using System;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.IO.Pipes;
using System.IO.Ports;

namespace RockSatX
{
    class mainProgram               
    {
        static void Main(string[] args)
        {
            SerialPort wallops = new SerialPort("COM4", 19200); // connect to telemetry
            wallops.Open();

            wallops.WriteLine("I'm ready");

            IOcontroller io = new IOcontroller();
            io.initialize();

            Thread readAD = new Thread(new ThreadStart(io.connect)); //start reading sensors
            readAD.Start();
            
            
            
            Process vacuumplus = new Process();
            vacuumplus.StartInfo.FileName = "C:\\Program Files\\Extorr Inc\\Vacuum Plus.exe"; // start the RGA
            vacuumplus.Start();

            Thread.Sleep(10000);  

            Process scriptStart = new Process();
            scriptStart.StartInfo.FileName = "C:\\RSX\\startup.vbs";
            scriptStart.Start();

            Thread.Sleep(90000);

            Process script1 = new Process();
            script1.StartInfo.FileName = "C:\\RSX\\filaments_off.vbs";
            script1.Start();

            Thread.Sleep(10000);

            Console.WriteLine("waiting for G-switch!"); // debug
            wallops.WriteLine("waiting for G-switch!");
            while (io.ad[0] <= 1000) { /* wait for g-switch */}  

            DateTime zero = DateTime.Now; // launch time
            Console.WriteLine("launch!");
            wallops.WriteLine("launch!");
            
            Thread.Sleep(73000); //T+73s

            Process script2 = new Process();
            script2.StartInfo.FileName = "C:\\RSX\\filaments_on.vbs"; 
            script2.Start();
            wallops.WriteLine("FILAMENTS!! ON!");
            Console.WriteLine("filaments on!");
            
            Thread.Sleep(10000); //T+83s

            TimeSpan dwell = new TimeSpan(0,4,28);  // dwell = T+268s
            wallops.WriteLine( "Waiting for Wall-E" );
            while( io.ad[1] >= 30 && (DateTime.Now - zero < dwell) ){}
 
           if(DateTime.Now - zero < dwell)
            {
                
                wallops.WriteLine("Skirt off!");
                wallops.WriteLine("BOOMS OPEN UP!!");
                io.boomsOpen();

                // --- streaming starts --//
                Console.WriteLine("streaming!!"); // debug
                TimeSpan elapsetime = new TimeSpan();

                NamedPipeClientStream rga1 = new NamedPipeClientStream(".", "Rga1", PipeDirection.In); //pipe del rga1
                NamedPipeClientStream rga2 = new NamedPipeClientStream(".", "Rga2", PipeDirection.In); //pipe del rga2

                rga1.Connect();
                rga2.Connect();

                StreamReader stream1 = new StreamReader(rga1); //stream de xml del rga1
                StreamReader stream2 = new StreamReader(rga2); //stream de xml del rga2

                string filename1 = "rga1 - " + DateTime.Now.ToString("MM-dd-yy Hmm") + ".xml";
                string filename2 = "rga2 - " + DateTime.Now.ToString("MM-dd-yy Hmm") + ".xml";
          
                StreamWriter file1 = new StreamWriter(filename1); //archivos de data recibida
                StreamWriter file2 = new StreamWriter(filename2);

            
                string buffer1, buffer2;
                while ((elapsetime = DateTime.Now - zero) <= dwell)
                {
                    Console.WriteLine(elapsetime);

                    buffer1 = stream1.ReadLine();
                    file1.WriteLine(buffer1);
                    buffer1 = "data1" + buffer1;
                    wallops.WriteLine(buffer1);
                    Console.WriteLine(buffer1);

                    buffer2 = stream2.ReadLine();
                    file2.WriteLine(buffer2);
                    buffer2 = "data2" + buffer2;
                    wallops.WriteLine(buffer2);
                    Console.WriteLine(buffer2);
                }
    
                rga1.Close();
                rga2.Close();
                stream1.Close();
                stream2.Close();
                file1.Close();
                file2.Close();

                wallops.WriteLine("Booms closing!");
                io.boomsClose();
            }
            else
            {
                wallops.WriteLine("Skin didn't go off :(");
            }

            Console.WriteLine("shutdown!"); // debug
            wallops.WriteLine("SHUTDOWN!!");

            wallops.Close();
            readAD.Abort();

            
            Process shutdown = new Process();
            shutdown.StartInfo.FileName = "C:\\RSX\\shutdown.bat";
            shutdown.Start();
        }
    }
}
