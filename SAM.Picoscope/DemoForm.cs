using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using PS6000Imports;
using PS6000PinnedArray;
using PicoStatus;
using ZedGraph;

namespace SAM.Picoscope
{
    public partial class DemoForm : Form
    {
        public const int BUFFER_SIZE = 50000;
        public const int MAX_CHANNELS = 4;
        public const int QUAD_SCOPE = 4;
        public const int DUAL_SCOPE = 2;

        uint _timebase = 8;
        short _oversample = 1;
        bool _scaleVoltages = true;

        ushort[] inputRanges = { 10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000, 20000, 50000 };
        bool _ready;
        short _trig = 0;
        uint _trigAt = 0;
        int _sampleCount;
        uint _startIndex;
        bool _autoStop;

        short[][] appBuffers;
        short[][] buffers;

        private ChannelSettings[] _channelSettings;
        private int _channelCount;
        private Imports.Range _firstRange;
        private Imports.Range _lastRange;
        private Imports.ps6000BlockReady _callbackDelegate;

        public DemoForm()
        {
            InitializeComponent();
        }

        private void cbbDevices_DropDown(object sender, EventArgs e)
        {
            cbbDevices.Items.Clear();
            
            short count;
            short serialsLength = 40;
            var serials = new StringBuilder(serialsLength);

            uint status = Imports.EnumerateUnits(out count, serials, ref serialsLength);
            if(status == Imports.PICO_OK)
            {
                cbbDevices.Items.Add(serials);
            }
        }


        private short _handle;
        private int timeInterval;
        private PinnedArray<short>[] maxPinned;
        private PinnedArray<short>[] minPinned;
        private short[] _minBuffers;
        private short[] _maxBuffers;

        private void cbbDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cbbDevices.Items.Count<0)
                return;

            UpdateStatus("Opening the device...");
            var status = Imports.OpenUnit(out _handle, null);
            UpdateStatus(string.Format("Handle: {0}", _handle));

            if(status != StatusCodes.PICO_OK)
            {
                UpdateStatus(string.Format("Unable to open device. Error code: {0}", status));
                return;
            }
        
            UpdateStatus("Device opened successfully!");
            GetDeviceInfo();
        }

        private void UpdateStatus(string msg)
        {
            tbStatus.Text = msg;
        }

        private void DemoForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Imports.CloseUnit(_handle);
        }

        void GetDeviceInfo()
        {
            string[] description = {
                           "Driver Version",
                           "USB Version",
                           "Hardware Version",
                           "Variant Info",
                           "Serial",
                           "Cal Date",
                           "Kernel Version",
                           "Digital H/W",
                           "Analogue H/W",
                           "Firmware Version 1",
                           "Firmware Version 2"
                         };

            var line = new StringBuilder(80);

            if (_handle >= 0)
            {
                for (uint i = 0; i < 11; i++)
                {
                    short requiredSize;
                    Imports.GetUnitInfo(_handle, line, 80, out requiredSize, i);
                    Console.WriteLine("{0}: {1}", description[i], line);

                    if (i == 3)
                    {
                        _channelSettings = new ChannelSettings[MAX_CHANNELS];
                        if (line[3] != 7)
                        {
                            _firstRange = Imports.Range.Range_50MV;
                            _lastRange = Imports.Range.Range_20V;

                            for (int j = 0; j < MAX_CHANNELS; j++)
                            {
                                _channelSettings[j].enabled = (j == 0);
                                _channelSettings[j].DCcoupled = Imports.PS6000Coupling.PS6000_DC_1M;
                                _channelSettings[j].range = Imports.Range.Range_5V;
                            }

                        }
                        _channelCount = int.Parse(line[1].ToString());
                    }
                }
            }
        }

        private void btnGetData_Click(object sender, EventArgs e)
        {
            if (btnGetData.Text == "Get Data")
            {
                RefreshDataGraph();

                //CollectBlockImmediate();
                //CollectStreamingTriggered();
                //CollectBlockTriggered();
                CollectBlockRapid();
                timerStreamData.Enabled = true;
                btnGetData.Text = "Stop";
            }
            else
            {
                timerStreamData.Enabled = false;
                Imports.Stop(_handle);
                btnGetData.Text = "Get Data";

                if(minPinned!=null)
                    foreach (PinnedArray<short> p in minPinned.Where(p => p != null))
                        p.Dispose();

                if(maxPinned != null)
                    foreach (PinnedArray<short> p in maxPinned.Where(p => p != null))
                        p.Dispose();
            }

        }

        private void CollectBlockTriggered()
        {
            _cmd = 'T';
            SetAuxTrigger();
            BlockDataHandler("Ten readings after trigger", 0);
        }

        private void SetAuxTrigger()
        {
            short triggerVoltage = mv_to_adc(1000, (short) Imports.Range.Range_5V); // ChannelInfo stores ADC counts

            Imports.TriggerChannelProperties[] sourceDetails = new[]
                                                                   {
                                                                       new Imports.TriggerChannelProperties(triggerVoltage,
                                                                                                            256*10,
                                                                                                            triggerVoltage,
                                                                                                            256*10,
                                                                                                            Imports.Channel.Aux,
                                                                                                            Imports.ThresholdMode.Level)
                                                                   };

            Imports.TriggerConditions[] conditions = new[]
                                                         {
                                                             new Imports.TriggerConditions(Imports.TriggerState.DontCare,
                                                                                           Imports.TriggerState.DontCare,
                                                                                           Imports.TriggerState.DontCare,
                                                                                           Imports.TriggerState.DontCare,
                                                                                           Imports.TriggerState.DontCare,
                                                                                           Imports.TriggerState.True,
                                                                                           Imports.TriggerState.DontCare)
                                                         };

            Imports.ThresholdDirection[] directions = new[]
                                                          {
                                                              Imports.ThresholdDirection.Rising,
                                                              Imports.ThresholdDirection.None,
                                                              Imports.ThresholdDirection.None,
                                                              Imports.ThresholdDirection.None,
                                                              Imports.ThresholdDirection.Rising,
                                                              Imports.ThresholdDirection.None
                                                          };

            UpdateStatus(string.Format("Collects when value rises past {0}mV",
                                       adc_to_mv(sourceDetails[0].ThresholdMajor, (int) Imports.Range.Range_5V)));

            SetDefaults();

            SetTrigger(sourceDetails, 1, conditions, 1, directions, null, 0, 0, 5000);
        }

        private void CollectStreamingTriggered()
        {
            _cmd = 'W';
            SetAuxTrigger();
            StreamDataHandler(100000);
        }

        /****************************************************************************
        * Stream Data Handler
        * - Used by the two stream data examples - untriggered and triggered
        * Inputs:
        * - unit - the unit to sample on
        * - preTrigger - the number of samples in the pre-trigger phase 
        *					(0 if no trigger has been set)
        ***************************************************************************/
        void StreamDataHandler(uint preTrigger)
        {
            appBuffers = new short[2][];
            buffers = new short[2][];

            uint sampleCount = BUFFER_SIZE * 100; /*  *100 is to make sure buffer large enough */
            buffers[0] = new short[sampleCount];
            buffers[1] = new short[sampleCount];

            appBuffers[0] = new short[sampleCount];
            appBuffers[1] = new short[sampleCount];

            Imports.SetDataBuffers(_handle, (Imports.Channel.ChannelA), buffers[0], buffers[1], sampleCount,
                                   Imports.PS6000DownSampleRatioMode.PS6000_RATIO_MODE_NONE);

            _autoStop = false;
            uint sampleInterval = 1;
            Imports.RunStreaming(_handle, ref sampleInterval, Imports.ReportedTimeUnits.MicroSeconds,
                                 preTrigger, 1000000-preTrigger, 0, 1,
                                 Imports.PS6000DownSampleRatioMode.PS6000_RATIO_MODE_NONE, sampleCount);

            UpdateStatus("Streaming data...");
        }

        /****************************************************************************
        * Callback
        * used by PS6000 data streaming collection calls, on receipt of data.
        * used to set global flags etc. checked by user routines
    ****************************************************************************/
        void StreamingCallback(short handle,
                                int noOfSamples,
                                uint startIndex,
                                short ov,
                                uint triggerAt,
                                short triggered,
                                short autoStop,
                                IntPtr pVoid)
        {
            // used for streaming
            _sampleCount = noOfSamples;
            _startIndex = startIndex;
            _autoStop = autoStop != 0;
            _ready = true;

            // flags to show if & where a trigger has occurred
            _trig = triggered;
            _trigAt = triggerAt;

            if (_sampleCount != 0)
            {
                for (int ch = 0; ch < _channelCount * 2; ch += 2)
                {
                    if (_channelSettings[(int)(Imports.Channel.ChannelA + (ch / 2))].enabled)
                    {
                        Array.Copy(buffers[ch], _startIndex, appBuffers[ch], _startIndex, _sampleCount); //max
                        Array.Copy(buffers[ch + 1], _startIndex, appBuffers[ch + 1], _startIndex, _sampleCount); //min
                    }
                }
            }
        }
        /****************************************************************************
         * mv_to_adc
         *
         * Convert a millivolt value into a 16-bit ADC count
         *
         *  (useful for setting trigger thresholds)
         ****************************************************************************/
        short mv_to_adc(short mv, short ch)
        {
            return (short)((mv * Imports.MaxValue) / inputRanges[ch]);
        }

        private void SetSignalGenerator()
        {
            uint pkpk = 2000000; // +/- 1V
            Imports.PS6000ExtraOperations operation;
            int offset = 0;
            double frequency = double.Parse(tbFrequency.Text);

            //Console.WriteLine("Signal Generator\n================\n");
            //Console.WriteLine("0:\tSINE      \t6:\tGAUSSIAN");
            //Console.WriteLine("1:\tSQUARE    \t7:\tHALF SINE");
            //Console.WriteLine("2:\tTRIANGLE  \t8:\tDC VOLTAGE");
            //Console.WriteLine("3:\tRAMP UP   \t9:\tWHITE NOISE");
            //Console.WriteLine("4:\tRAMP DOWN");
            //Console.WriteLine("5:\tSINC");

            short waveform = (short)(cbbSignalType.SelectedIndex);

            switch (cbbSignalType.SelectedIndex)
            {
                case 8:
                    offset = 1000;
                    operation = Imports.PS6000ExtraOperations.PS6000_ES_OFF;
                    break;

                case 9:
                    operation = Imports.PS6000ExtraOperations.PS6000_WHITENOISE;
                    break;

                default:
                    operation = Imports.PS6000ExtraOperations.PS6000_ES_OFF;
                    offset = 0;
                    break;
                case 10:
                    offset = 0;
                    pkpk = 0;
                    operation = Imports.PS6000ExtraOperations.PS6000_ES_OFF;
                    waveform = 0;
                    break;
            }

            uint status = Imports.SetSigGenBuiltInV2(_handle, offset, pkpk, (Imports.WaveType)waveform, frequency, frequency, 0, 0, 0, operation, 0, 0, 0, 0, 0);
            UpdateStatus(status != StatusCodes.PICO_OK
                             ? string.Format("SetSigGenBuiltIn: Status Error {0}", status)
                             : "Generated Signal Successfully");
        }

        private void RefreshDataGraph()
        {
            graphData.GraphPane.XAxis.Title.Text = "Time";
            graphData.GraphPane.YAxis.Title.Text = "Amplitude";
            //graphData.GraphPane.YAxis.Scale.Max = 600;
            //graphData.GraphPane.YAxis.Scale.Min = -600;
        }

        private void CollectBlockImmediate()
        {
            SetDefaults();

            /* Trigger disabled	*/
            SetTrigger(null, 0, null, 0, null, null, 0, 0, 0);

            BlockDataHandler("First 10 readings", 0);
        }

        void SetDefaults()
        {
            for (int i = 0; i < _channelCount; i++) // reset channels to most recent settings
            {
                Imports.SetChannel(_handle, Imports.Channel.ChannelA + i,
                                               (short)(_channelSettings[(int)(Imports.Channel.ChannelA + i)].enabled ? 1 : 0),
                                                _channelSettings[i].DCcoupled,
                                               _channelSettings[(int)(Imports.Channel.ChannelA + i)].range, 0, Imports.PS6000BandwidthLimiter.PS6000_BW_FULL);
            }
        }

        /****************************************************************************
        *  SetTrigger
        *  this function sets all the required trigger parameters, and calls the 
        *  triggering functions
        ****************************************************************************/
        uint SetTrigger(Imports.TriggerChannelProperties[] channelProperties, short nChannelProperties, Imports.TriggerConditions[] triggerConditions, short nTriggerConditions, Imports.ThresholdDirection[] directions, Pwq pwq, uint delay, short auxOutputEnabled, int autoTriggerMs)
        {
            uint status = Imports.SetTriggerChannelProperties(_handle, channelProperties, nChannelProperties,
                                                              auxOutputEnabled, autoTriggerMs);

            if (status != StatusCodes.PICO_OK)
            {
                return status;
            }

            status = Imports.SetTriggerChannelConditions(_handle, triggerConditions, nTriggerConditions);

            if (status != StatusCodes.PICO_OK)
            {
                return status;
            }

            if (directions == null)
            {
                directions = new[] { Imports.ThresholdDirection.None,
                                                    Imports.ThresholdDirection.None, Imports.ThresholdDirection.None,
                                                    Imports.ThresholdDirection.None, Imports.ThresholdDirection.None,
                                                    Imports.ThresholdDirection.None};
            }

            status = Imports.SetTriggerChannelDirections(_handle,
                                                                directions[(int)Imports.Channel.ChannelA],
                                                                directions[(int)Imports.Channel.ChannelB],
                                                                directions[(int)Imports.Channel.ChannelC],
                                                                directions[(int)Imports.Channel.ChannelD],
                                                                directions[(int)Imports.Channel.External],
                                                                directions[(int)Imports.Channel.Aux]);

            if (status != StatusCodes.PICO_OK)
            {
                return status;
            }

            status = Imports.SetTriggerDelay(_handle, delay);

            if (status != StatusCodes.PICO_OK)
            {
                return status;
            }

            if (pwq == null)
            {
                pwq = new Pwq(null, 0, Imports.ThresholdDirection.None, 0, 0, Imports.PulseWidthType.None);
            }

            status = Imports.SetPulseWidthQualifier(_handle, pwq.conditions,
                                                    pwq.nConditions, pwq.direction,
                                                    pwq.lower, pwq.upper, pwq.type);
            return status;
        }
        /****************************************************************************
        * BlockDataHandler
        * - Used by all block data routines
        * - acquires data (user sets trigger mode before calling), displays 10 items
        *   and saves all to data.txt
        * Input :
        * - unit : the unit to use.
        * - text : the text to display before the display of data slice
        * - offset : the offset into the data buffer to start the display's slice.
       ****************************************************************************/
        void BlockDataHandler(string text, int offset)
        {
            uint sampleCount = BUFFER_SIZE;
            minPinned = new PinnedArray<short>[_channelCount];
            maxPinned = new PinnedArray<short>[_channelCount];

            int timeIndisposed;

            for (int i = 0; i < _channelCount; i++)
            {
                _minBuffers = new short[sampleCount];
                _maxBuffers = new short[sampleCount];
                minPinned[i] = new PinnedArray<short>(_minBuffers);
                maxPinned[i] = new PinnedArray<short>(_maxBuffers);
                Imports.SetDataBuffers(_handle, (Imports.Channel)i, _maxBuffers, _minBuffers, sampleCount, Imports.PS6000DownSampleRatioMode.PS6000_RATIO_MODE_NONE);
            }

            /*  Find the maximum number of samples, the time interval (in timeUnits),
             *		 the most suitable time units at the current _timebase
             */
            uint maxSamples;

            while (Imports.GetTimebase(_handle, _timebase, sampleCount, out timeInterval, _oversample, out maxSamples, 0) != 0)
            {
                _timebase++;
            }

            UpdateStatus(string.Format("Timebase: {0}\toversample:{1}", _timebase, _oversample));

            /* Start it collecting, then wait for completion*/
            _ready = false;
            _callbackDelegate = BlockCallback;
            Imports.RunBlock(_handle, PRE_TRIG_SAMPLES, BUFFER_SIZE - PRE_TRIG_SAMPLES, _timebase, _oversample, out timeIndisposed, 0, _callbackDelegate, IntPtr.Zero);
        }

        /****************************************************************************
        * Callback
        * used by ps6000 data block collection calls, on receipt of data.
        * used to set global flags etc checked by user routines
        ****************************************************************************/
        void BlockCallback(short handle, uint status, IntPtr pVoid)
        {
            // flag to say done reading data
            _ready = true;
        }

        /****************************************************************************
        * adc_to_mv
        *
        * Convert an 16-bit ADC count into millivolts
        ****************************************************************************/
        int adc_to_mv(int raw, int ch)
        {
            return (raw * inputRanges[ch]) / Imports.MaxValue;
        }

        private char _cmd = 'B';
        private uint nRapidCaptures;
        private uint numChannels;
        private uint numSamples;

        private void timerStreamData_Tick(object sender, EventArgs e)
        {
            if(btnGetData.Text !="Stop")
            {
                timerStreamData.Enabled = false;
                return;
            }

            switch (_cmd)
            {
                case 'B':   // Immediate block
                    if (!_ready)
                    {
                        UpdateStatus("Waiting for data...");
                        Thread.Sleep(50);
                    }
                    else
                    {
                        timerStreamData.Enabled = false;
                        SaveAndPlotBlock();
                    }
                    break;

                case 'W':   // Triggered streaming
                    PlotStream();
                    break;

                case'T':
                    uint sampleCount = BUFFER_SIZE;
                    if (_ready)
                    {
                        short overflow;
                        Imports.GetValues(_handle, 0, ref sampleCount, 1, Imports.PS6000DownSampleRatioMode.PS6000_RATIO_MODE_NONE, 0, out overflow);

                        PlotBlock();
                        int timeIndisposed;
                        Imports.RunBlock(_handle, PRE_TRIG_SAMPLES, BUFFER_SIZE - PRE_TRIG_SAMPLES, _timebase, _oversample, out timeIndisposed, 0, _callbackDelegate, IntPtr.Zero);
                    }
                    break;
                case 'R':
                    if(!_ready)
                        return;
                    Imports.Stop(_handle);

                    // Set up the data arrays and pin them
                    var pinned = SetupPinnedDataArrays();

                    // Read the data
                    short[] overflows = new short[nRapidCaptures];

                    Imports.GetValuesRapid(_handle, ref numSamples, 0, nRapidCaptures - 1, 1, Imports.PS6000DownSampleRatioMode.PS6000_RATIO_MODE_NONE, overflows);

                    /* Print out the first 10 readings, converting the readings to mV if required */

                    var pairList = new PointPairList();
                    for (int i = 0; i < pinned[499].Target.Length; i++)
                    {
                        var iVal = adc_to_mv(pinned[499].Target[i], (int)_channelSettings[(int)(Imports.Channel.ChannelA)].range); 
                        pairList.Add(i,iVal);
                    }

                    graphData.GraphPane.CurveList.Clear();
                    graphData.GraphPane.AddCurve("Data", pairList, Color.Blue, SymbolType.None);
                    graphData.AxisChange();
                    graphData.Invalidate();

                    _ready = false;
                    int timeIndispos;
                    Imports.RunBlock(_handle, PRE_TRIG_SAMPLES, BUFFER_SIZE - PRE_TRIG_SAMPLES, _timebase, _oversample, out timeIndispos, 0, _callbackDelegate, IntPtr.Zero);
                    // Un-pin the arrays
                    foreach (PinnedArray<short> p in pinned)
                    {
                        if (p != null)
                            p.Dispose();
                    }
                    break;
            }
        }

        private PinnedArray<short>[] SetupPinnedDataArrays()
        {
            short[][] values = new short[nRapidCaptures][];
            PinnedArray<short>[] pinned = new PinnedArray<short>[nRapidCaptures];

            for (ushort segment = 0; segment < nRapidCaptures; segment++)
            {
                values[segment] = new short[numSamples];
                if (_channelSettings[(int)Imports.Channel.ChannelA].enabled)
                {
                    values[segment] = new short[numSamples];
                    pinned[segment] = new PinnedArray<short>(values[segment]);

                    Imports.SetDataBuffersRapid(_handle,
                                                Imports.Channel.ChannelA,
                                                values[segment],
                                                numSamples,
                                                segment,
                                                Imports.PS6000DownSampleRatioMode.PS6000_RATIO_MODE_NONE);
                }
            }
            return pinned;
        }

        private void PlotStream()
        {
            if(btnGetData.Text =="Get Data")
            {
                timerStreamData.Enabled = false;
                return;
            }

            /* Poll until data is received. Until then, GetStreamingLatestValues wont call the callback */
            //Thread.Sleep(100);
            _ready = false;
            uint status = Imports.GetStreamingLatestValues(_handle, StreamingCallback, IntPtr.Zero);

            if (status > 0 && status != 39)
                UpdateStatus(string.Format("Status =  {0}\n", status));

            if(_autoStop)
                return;

            if (_ready && _sampleCount > 0) /* can be ready and have no data, if autoStop has fired */
            {
                uint triggeredAt = 0;
                int totalSamples = 0;
                if (_trig > 0)
                    triggeredAt = (uint)totalSamples + _trigAt;

                totalSamples += _sampleCount;
                UpdateStatus(string.Format("\nCollected {0,4} samples, index = {1,5}, Total = {2,5}", _sampleCount, _startIndex, totalSamples));

                if (_trig > 0)
                    UpdateStatus(string.Format("\tTrig at Index {0}", triggeredAt));

                var dataList = new PointPairList();
                for (uint i = 0; i <  _sampleCount; i++)
                {
                    dataList.Add(i, adc_to_mv(appBuffers[0][i],(int)_channelSettings[(int)Imports.Channel.ChannelA].range));
                }

                for (uint i = 0; i < _sampleCount; i++)
                {
                    dataList.Add(i,adc_to_mv(appBuffers[0 + 1][i],(int) _channelSettings[(int) Imports.Channel.ChannelA].range));
                }

                graphData.GraphPane.CurveList.Clear();
                var line = graphData.GraphPane.AddCurve("Streaming data", dataList, Color.Blue, SymbolType.Circle);
                line.Line.IsVisible = false;
                graphData.AxisChange();
                graphData.Invalidate();
            }
        }

        private void SaveAndPlotBlock()
        {
            btnGetData.Text = "Get Data";
            Imports.Stop(_handle);

            uint sampleCount = BUFFER_SIZE;
            if (_ready)
            {
                short overflow;
                Imports.GetValues(_handle, 0, ref sampleCount, 1,
                                  Imports.PS6000DownSampleRatioMode.PS6000_RATIO_MODE_NONE, 0, out overflow);

                sampleCount = Math.Min(sampleCount, BUFFER_SIZE);
                SaveBlock(sampleCount);
                PlotBlock();
            }

            foreach (PinnedArray<short> p in minPinned.Where(p => p != null))
            {
                p.Dispose();
            }

            foreach (PinnedArray<short> p in maxPinned.Where(p => p != null))
            {
                p.Dispose();
            }
        }

        private void PlotBlock()
        {
            graphData.GraphPane.CurveList.Clear();
            for (int ch = 0; ch < _channelCount; ch++)
            {
                var pairList = new PointPairList();
                for (int j = 0; j < maxPinned[ch].Target.Length; j++)
                {
                    pairList.Add(j, maxPinned[ch].Target[j]);
                }

                switch (ch)
                {
                    case 0:
                        graphData.GraphPane.AddCurve(string.Format("Channel A"), pairList, Color.Blue, SymbolType.None);
                        break;
                    case 1:
                        graphData.GraphPane.AddCurve(string.Format("Channel B"), pairList, Color.Red, SymbolType.Circle);
                        break;
                    case 2:
                        graphData.GraphPane.AddCurve(string.Format("Channel C"), pairList, Color.DarkGreen,
                                                     SymbolType.Square);
                        break;
                    case 3:
                        graphData.GraphPane.AddCurve(string.Format("Channel D"), pairList, Color.DarkGoldenrod,
                                                     SymbolType.Triangle);
                        break;
                }
            }
            graphData.AxisChange();
            graphData.Invalidate();
        }

        private void SaveBlock(uint sampleCount)
        {
            TextWriter writer = new StreamWriter("block.txt", false);

            writer.Write("For each of the enabled Channels, results shown are....");
            writer.WriteLine();
            writer.WriteLine(
                "Time interval Maximum Aggregated value ADC Count & mV, Minimum Aggregated value ADC Count & mV");
            writer.WriteLine();

            writer.Write("Time  ");

            for (int ch = 0; ch < _channelCount; ch++)
            {
                if (_channelSettings[ch].enabled)
                {
                    writer.Write("Ch  Max ADC    Max mV   Min ADC    Min mV   ");
                }
            }
            writer.WriteLine();

            for (int i = 0; i < sampleCount; i++)
            {
                writer.Write("{0,4}  ", (i*timeInterval));

                for (int ch = 0; ch < _channelCount; ch++)
                {
                    if (_channelSettings[ch].enabled)
                    {
                        writer.Write("Ch{0} {1,7}   {2,7}   {3,7}   {4,7}   ",
                                     (char) ('A' + ch),
                                     maxPinned[ch].Target[i],
                                     adc_to_mv(maxPinned[ch].Target[i],
                                               (int) _channelSettings[(int) (Imports.Channel.ChannelA + ch)].range),
                                     minPinned[ch].Target[i],
                                     adc_to_mv(minPinned[ch].Target[i],
                                               (int) _channelSettings[(int) (Imports.Channel.ChannelA + ch)].range));
                    }
                }
                writer.WriteLine();
            }

            writer.Close();
        }

        private void TurnOffSignalGeneration()
        {
            Imports.SetSigGenBuiltInV2(_handle, 0, 0, 0, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            UpdateStatus("Turned off signal generation");
        }

        /****************************************************************************
     *
     * Select _timebase, set _oversample to on and time units as nano seconds
     *
     ****************************************************************************/
        void SetTimebase()
        {
            uint maxSamples;
            bool valid;

            do
            {
                UpdateStatus("Set sample rate: " + cbbSampleRate.Text);
                try
                {
                    _timebase = (uint)cbbSampleRate.SelectedIndex;
                    valid = true;
                }
                catch (FormatException e)
                {
                    valid = false;
                    UpdateStatus("Error: " + e.Message);
                }

            } while (!valid);

            while (Imports.GetTimebase(_handle, _timebase, BUFFER_SIZE, out timeInterval, 1, out maxSamples, 0) != 0)
            {
                UpdateStatus(string.Format("Selected timebase {0} could not be used ", _timebase));
                _timebase++;
            }

            UpdateStatus(string.Format("Using Timebase {0} - {1} ns sampleinterval", _timebase, timeInterval));
            _oversample = 1;
        }

        private void cbbSignalType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cbbSignalType.Text == "None")
            {
                TurnOffSignalGeneration();
            }
            else
            {
                SetSignalGenerator();
            }
        }

        private void cbbSampleRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetTimebase();
        }

        private void tbFrequency_TextChanged(object sender, EventArgs e)
        {
            double fre;
            if (double.TryParse(tbFrequency.Text, out fre))
                SetSignalGenerator();
        }

        /****************************************************************************
       *  CollectBlockRapid
       *  this function demonstrates how to collect blocks of data
       * using the RapidCapture function
       ****************************************************************************/
        void CollectBlockRapid()
        {
            _cmd = 'R';
            nRapidCaptures = uint.Parse(tbFrequency.Text);
            Imports.SetNoOfRapidCaptures(_handle, nRapidCaptures);

            uint maxSamples;
            uint status = Imports.MemorySegments(_handle, nRapidCaptures, out maxSamples);
            UpdateStatus(status != StatusCodes.PICO_OK ? "Error:" + status : "");

            SetAuxTrigger();

            RapidBlockDataHandler();
        }

        private const int PRE_TRIG_SAMPLES = 50;
        /****************************************************************************
       * RapidBlockDataHandler
       * - Used by all the CollectBlockRapid routine
       * - acquires data (user sets trigger mode before calling), displays 10 items
       * Input :
       * - nRapidCaptures : the user specified number of blocks to capture
       ****************************************************************************/
        private void RapidBlockDataHandler()
        {
            numChannels = (uint)_channelCount;
            numSamples = BUFFER_SIZE;

            // Run the rapid block capture
            int timeIndisposed;
            _ready = false;

            // Find the maximum number of samples and the time interval (in nanoseconds), if the timebase index is valid
            uint maxSamples;
            var status = Imports.GetTimebase(_handle, _timebase, numSamples, out timeInterval, _oversample, out maxSamples, 0);
            //while (tb != 0)
            //{
            //    _timebase = tb;
            //    tb = Imports.GetTimebase(_handle, _timebase, numSamples, out timeInterval, _oversample, out maxSamples, 0);
            //}
            _callbackDelegate = BlockCallback;

            status = Imports.RunBlock(_handle, PRE_TRIG_SAMPLES, BUFFER_SIZE - PRE_TRIG_SAMPLES, _timebase, _oversample, out timeIndisposed, 0, _callbackDelegate, IntPtr.Zero);

            if(status != StatusCodes.PICO_OK)
            {
                UpdateStatus("Rapid block data setting error");
            }
        }
    }

    struct ChannelSettings
    {
        public Imports.PS6000Coupling DCcoupled;
        public Imports.Range range;
        public bool enabled;
    }

    class Pwq
    {
        public Imports.PwqConditions[] conditions;
        public short nConditions;
        public Imports.ThresholdDirection direction;
        public uint lower;
        public uint upper;
        public Imports.PulseWidthType type;

        public Pwq(Imports.PwqConditions[] conditions,
            short nConditions,
            Imports.ThresholdDirection direction,
            uint lower, uint upper,
            Imports.PulseWidthType type)
        {
            this.conditions = conditions;
            this.nConditions = nConditions;
            this.direction = direction;
            this.lower = lower;
            this.upper = upper;
            this.type = type;
        }
    }
}
