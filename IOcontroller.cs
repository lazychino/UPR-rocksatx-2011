using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

class IOcontroller
{
    public class Relay
    {
        public bool state = false;
        public bool trigger = false;
    }

    public int[] ad = new int[] {};  // array contains the values of all 8 analog to digital converter in 10bit resolution

    Relay[] relay = new Relay[4]; // array contains states of all 4 relays to be assign when its pair on trigger array its true
        
    
    public void initialize()
    {
        for( int n = 0; n < 4; n++ )
        {
            relay[n] = new Relay();
            relay[n].state = false;
            relay[n].trigger = false;
        }
    }
        
    public void connect() // this method is a thread that exclusively communicate with the NCD control board 
    {
        
        NCD.NCDComponent io = new NCD.NCDComponent();
        io.BaudRate = 115200;
        io.PortName = "COM3";
        io.OpenPort();
        io.ProXR.RelayBanks.TurnOffAllRelays();
        //Console.WriteLine("starting reading sensor"); // debug
        while( true )
        {
            if (io.IsOpen)
            {
                ad = io.ProXR.AD8.ReadAllChannels10BitsValues();
                
                //int i = 1;
                //Console.Clear();
                /*foreach( int n in ad )
                {
                    Console.WriteLine("ad{0}: {1}", i, n);
                    i++;
                }  */
                   
                    
                if ( (relay[0].trigger) || (relay[1].trigger) || (relay[2].trigger) || (relay[3].trigger) )
                {
                    for (int n = 0; n < 4; n++)
                    {
                        if (relay[n].state && relay[n].trigger)
                        {
                            io.ProXR.RelayBanks.TurnOnRelay((byte)n);
                        }
                        else if (!relay[n].state && relay[n].trigger)
                        {
                            io.ProXR.RelayBanks.TurnOffRelay((byte)n);
                        }
                        else { }
                    }
                }
            }
            else
            {
                Console.WriteLine("no io board");
            }
            Thread.Sleep( 50 );
        }
    } 

    public void boomsOpen()
    {
        bool boom1 = true, 
            boom2 = true;
        this.startOpeningBoom1();
        this.startOpeningBoom2();

        while( boom1 || boom2 )
        {
            if( this.ad[3] >= 805 )
            {
                this.stopOpeningBoom1();
                boom1 = false;
            }
            if( this.ad[4] >= 985 )
            {
                this.stopOpeningBoom2();
                boom2 = false;
            }
        }
    }

    public void boomsClose()
    {
        bool boom1 = true,
            boom2 = true;
        this.startClosingBoom1();
        this.startClosingBoom2();

        while( boom1 || boom2 )
        {
            if( this.ad[3] <= 155 )
            {
                this.stopClosingBoom1();
                boom1 = false;
            }
            if( this.ad[4] <= 480 )
            {
                this.stopClosingBoom2();
                boom2 = false;
            }
        }
    }

    void startOpeningBoom1()
    {
        relay[0].state = true;

        relay[0].trigger = true;
        Thread.Sleep(500);
        relay[0].trigger = false;
    }

    void stopOpeningBoom1()
    {
        relay[0].state = false;

        relay[0].trigger = true;
        Thread.Sleep(500);
        relay[0].trigger = false;
    }

    void startOpeningBoom2()
    {
        relay[2].state = true;

        relay[2].trigger = true;
        Thread.Sleep(500);
        relay[2].trigger = false;
    }

    void stopOpeningBoom2()
    {
        relay[2].state = false;

        relay[2].trigger = true;
        Thread.Sleep(500);
        relay[2].trigger = false;
    }

    void startClosingBoom1()
    {
        relay[1].state = true;

        relay[1].trigger = true;
        Thread.Sleep(500);
        relay[1].trigger = false;
    }

    void stopClosingBoom1()
    {
        relay[1].state = false;

        relay[1].trigger = true;
        Thread.Sleep(500);
        relay[1].trigger = false;
    }

    void startClosingBoom2()
    {
        relay[3].state = true;

        relay[3].trigger = true;
        Thread.Sleep(500);
        relay[3].trigger = false;
    }
    
    void stopClosingBoom2()
    {
        relay[3].state = false;

        relay[3].trigger = true;
        Thread.Sleep(500);
        relay[3].trigger = false;
    }
}
