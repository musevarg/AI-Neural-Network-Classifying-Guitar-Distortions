using Jacobi.Vst.Core;
using Jacobi.Vst.Host.Interop;
using System;
using System.Linq;
using System.Diagnostics;
using NAudio.Wave;

namespace GUI2
{
    class LoadVst
    {
        public VstPluginContext PluginContext { get; set; }
        public Jacobi.Vst.Core.Host.IVstPluginCommandStub PluginCommandStub { get; set; }

        public LoadVst()
        {
            PluginContext = OpenPlugin(@AppDomain.CurrentDomain.BaseDirectory + "AmplifikationLite.dll");
        }

        private VstPluginContext OpenPlugin(string pluginPath)
        {
            try
            {
                HostCommandStub hostCmdStub = new HostCommandStub();
                hostCmdStub.PluginCalled += new EventHandler<PluginCalledEventArgs>(HostCmdStub_PluginCalled);

                VstPluginContext ctx = VstPluginContext.Create(pluginPath, hostCmdStub);

                // add custom data to the context
                ctx.Set("PluginPath", pluginPath);
                ctx.Set("HostCmdStub", hostCmdStub);

                // actually open the plugin itself
                ctx.PluginCommandStub.Commands.Open();

                return ctx;
            }
            catch (Exception e)
            {
                Debug.WriteLine("VST LOADING ERROR: " + e.ToString());
            }

            return null;
        }

        private void HostCmdStub_PluginCalled(object sender, PluginCalledEventArgs e)
        {
            HostCommandStub hostCmdStub = (HostCommandStub)sender;

            // can be null when called from inside the plugin main entry point.
            if (hostCmdStub.PluginContext.PluginInfo != null)
            {
                Debug.WriteLine("Plugin " + hostCmdStub.PluginContext.PluginInfo.PluginID + " called: " + e.Message);
            }
            else
            {
                Debug.WriteLine("The loading Plugin called: " + e.Message);
            }
        }


        /* ATTEMPT TO RUN AUDIO THROUGH VST PLUGIN
         * The only success so far is with the microphone's direct input.
         * No way to route audio file through it so far.
         */


        BufferedWaveProvider waveProvider;
        BufferedWaveProvider waveOutProv;
        WaveOut waveOut;
        byte[] naudioBuf;
        VstAudioBuffer[] vstBufIn;
        VstAudioBuffer[] vstBufOut;

        public void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveProvider != null)
            {
                waveProvider.AddSamples(e.Buffer, 0, e.Buffer.Length);
            }

            Debug.WriteLine("WHAT DO WE HAVE HERE?\n" + e.Buffer + "\n" + e.Buffer.Length);

            naudioBuf = e.Buffer;

            unsafe
            {
                int j = 0;
                for (int i = 0; i < e.Buffer.Length; i++)
                {
                    byte[] tmpbytearr = new byte[4];
                    tmpbytearr[0] = naudioBuf[i];
                    i++;
                    tmpbytearr[1] = naudioBuf[i];
                    Int16 tmpint = BitConverter.ToInt16(tmpbytearr, 0);
                    float f = (((float)tmpint / (float)Int16.MaxValue));
                    try
                    {
                        vstBufIn[0][j] = f;
                        vstBufIn[1][j] = f;
                    } catch (Exception ex){ Debug.WriteLine(ex.Message); }
                    j++;
                }
            }

            PluginContext.PluginCommandStub.Commands.ProcessReplacing(vstBufIn, vstBufOut);
            PluginContext.PluginCommandStub.Commands.EditorIdle();

            byte[] bytebuffer;
            unsafe
            {
                float* tmpBufL = ((IDirectBufferAccess32)vstBufOut[0]).Buffer;
                float* tmpBufR = ((IDirectBufferAccess32)vstBufOut[1]).Buffer;
                bytebuffer = new byte[vstBufOut[0].SampleCount * 2];
                int j = 0;
                for (int i = 0; i < (vstBufOut[0].SampleCount * 2); i++)
                {
                    Int16 tmpint = (Int16)((float)vstBufOut[1][j] * (float)Int16.MaxValue);
                    byte[] tmparr = BitConverter.GetBytes(tmpint);
                    bytebuffer[i] = tmparr[0];
                    i++;
                    bytebuffer[i] = tmparr[1];
                    tmpint = (Int16)((float)vstBufOut[1][j] * (float)Int16.MaxValue);
                    j++;
                }
            }
            waveOutProv.AddSamples(bytebuffer, 0, bytebuffer.Length);
            
        }

        public void ProccessVstAgainAgain()
        {
            /*WaveStream ws = new WaveFileReader(@"D:\Misc\Napier\Honours-Project\zpython\audio-files\1. Melodies\0. DI Tracks\session_Audio 1.wav");
            custombuffer = new byte[ws.Length];
            ws.Read(custombuffer, 0, custombuffer.Length);

           WaveStream waveIn = ws;*/

            WaveIn waveIn = new WaveIn();            
            waveIn.BufferMilliseconds = 25;
            waveIn.DataAvailable += waveIn_DataAvailable;

            // create wave provider
            waveProvider = new BufferedWaveProvider(waveIn.WaveFormat) { DiscardOnBufferOverflow = true };
            waveOutProv = new BufferedWaveProvider(waveIn.WaveFormat) { DiscardOnBufferOverflow = true };

            // create wave output to speakers
            waveOut = new WaveOut();
            waveOut.DesiredLatency = 100;
            waveOut.Init(waveOutProv);

            // start recording and playback
            waveIn.StartRecording();
            waveOut.Play();

            VstAudioBufferManager vstBufManIn = new VstAudioBufferManager(2, 200);
            VstAudioBufferManager vstBufManOut = new VstAudioBufferManager(2, 200);

            //VstAudioBufferManager vstBufManIn = new VstAudioBufferManager(PluginContext.PluginInfo.AudioInputCount, custombuffer.Length);
            //VstAudioBufferManager vstBufManOut = new VstAudioBufferManager(PluginContext.PluginInfo.AudioOutputCount, custombuffer.Length);

            vstBufIn = vstBufManIn.Buffers.ToArray();
            vstBufOut = vstBufManOut.Buffers.ToArray();

        }
        
    }
}
