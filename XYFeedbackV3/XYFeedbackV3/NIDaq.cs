﻿using System;
using System.Threading;
using NationalInstruments.DAQmx;

namespace XYFeedbackV3
{
    public class NIDaq
    {

        double NIMaxVolt = 10;

        //private NationalInstruments.DAQmx.Task USB6008_2_AITask;
        internal double TraLoc, LatLoc;
        Semaphore USB6008_Mutex = new Semaphore(1, 1);//I know, it is not a mutex, but had some weird issues using mutex across multiple threads so this is my solution to use a sempafore as a mutex

        string Device;

        private NationalInstruments.DAQmx.Task USB6008_AITask;
        private NationalInstruments.DAQmx.Task USB6008_AOTask;
        private NationalInstruments.DAQmx.Task USB6008_DOTask;
        private AnalogMultiChannelReader USB6008_Reader;
        private AnalogMultiChannelWriter USB6008_Analog_Writter;
        private DigitalSingleChannelWriter USB6008_Digital_Writter;

        public NIDaq()
        {
            TraLoc = 0;
            LatLoc = 0;
            Device = "dev2";
            USB6008_Mutex = new Semaphore(1, 1);//I know, it is not a mutex, but had some weird issues using mutex across multiple threads so this is my solution to use a sempafore as a mutex
            Setup_USB6008();
        }


        public double[] ReadUSBDa()
        {
            double[] data1 = { 0, 0 };
            try
            {
                USB6008_Mutex.WaitOne();
                data1 = USB6008_Reader.ReadSingleSample();
                USB6008_Mutex.Release();
            }
            catch (NationalInstruments.DAQmx.DaqException ex)
            {
                Console.WriteLine("usb6008 1 read error" + ex.Message.ToString());
                //error has occured need to reset daq
                Setup_USB6008();
                Thread.Sleep(50);
            }
            TraLoc = data1[0];
            LatLoc = data1[1];
            return data1;
        }

        public void Setup_USB6008()
        {

            //Resets and configures the NI USB6008 Daq boards
            Device dev = DaqSystem.Local.LoadDevice(Device);//added to reset the DAQ boards if they fail to comunicate giving error code 50405
            dev.Reset();
            AIChannel TransverseChannel, LateralChannel;
            try
            {
                //Setting up NI DAQ for Axial Force Measurment via Strain Circuit and current Measurment of Spindle Motor for torque 
                USB6008_AITask = new NationalInstruments.DAQmx.Task();

                TransverseChannel = USB6008_AITask.AIChannels.CreateVoltageChannel(
                    Device + "/ai0",  //Physical name of channel
                    "TransverseChannel",  //The name to associate with this channel
                    AITerminalConfiguration.Differential,  //Differential Wiring
                    -10,  //-10v minimum
                    10,  //10v maximum
                    AIVoltageUnits.Volts  //Use volts
                    );
                LateralChannel = USB6008_AITask.AIChannels.CreateVoltageChannel(
                   Device + "/ai1",  //Physical name of channel
                   "LateralChannel",  //The name to associate with this channel
                   AITerminalConfiguration.Differential,  //Differential Wiring
                   -10,  //-0.1v minimum
                   10,  //10v maximum
                   AIVoltageUnits.Volts  //Use volts
                   );
                USB6008_Reader = new AnalogMultiChannelReader(USB6008_AITask.Stream);
                ////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////
                USB6008_DOTask = new NationalInstruments.DAQmx.Task();
                USB6008_DOTask.DOChannels.CreateChannel(Device + "/port0", "port0", ChannelLineGrouping.OneChannelForAllLines);
                USB6008_Digital_Writter = new DigitalSingleChannelWriter(USB6008_DOTask.Stream);
            }
            catch (NationalInstruments.DAQmx.DaqException e)
            {
                Console.WriteLine("Error?\n\n" + e.ToString(), "NI USB 6008 1 Error");
            }
        }
    }
}
