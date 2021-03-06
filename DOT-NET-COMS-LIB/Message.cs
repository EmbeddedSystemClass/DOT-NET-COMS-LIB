﻿using System.Collections.Generic;

namespace DOT_NET_COMS_LIB
{
    public class Message
    {
        #region Variables
        public byte H = 0x48,
             Z = 0x5A,
             Length = 0,
             Destination,
             Source,
             Options,
             LSC,     // Least significance code byte.
             MSC,     // Most significance code byte.
             // Message byte array 
             // ...
             CRC;
        List<byte> AllMesssageList;
        byte[] Payload, AllMessage, OrganizedBuffer;
        #endregion

        /// <summary>
        /// The Hexabitz Buffer class wiki: https://hexabitz.com/docs/code-overview/array-messaging/
        /// The general constructor, all the payload parameters [Par1, Par2,...] must be included in the correct order within the Message array.
        /// </summary>
        public Message(byte Destination, byte Source,  byte Options, int Code, byte[] Payload)
        {
            this.Destination = Destination;
            this.Source = Source;
            this.Options = Options;
            LSC = (byte)(Code & 0xFF); // Get the MSC & LSC automaticly from the code.
            MSC = (byte)(Code >> 8);
            this.Payload = Payload;

            AllMesssageList = new List<byte>();
            AllMesssageList.Add(H);
            AllMesssageList.Add(Z);
            AllMesssageList.Add(Length);
            AllMesssageList.Add(Destination);
            AllMesssageList.Add(Source);
            AllMesssageList.Add(Options);
            AllMesssageList.Add(LSC);
            if(MSC != 0) // If the code is only one byte so the MSC
                AllMesssageList.Add(MSC);

            foreach (byte item in Payload)
            {
                AllMesssageList.Add(item);
            }

            Length = (byte)(AllMesssageList.Count - 3); // Not including H & Z delimiters, the length byte itself and the CRC byte
                                                      // so its 4 but we didn't add the CRC yet so its 3.
            AllMesssageList[2] = Length; // Replace it with the correct length value.
            CRC = GetCRC();
            AllMesssageList.Add(CRC);
            AllMessage = AllMesssageList.ToArray();
        }

        // Return the Cyclic Redundancy Check for the buffer.
        public byte GetCRC()
        {
            List<byte> organizedMesssageList = Organize(AllMesssageList); // Here we are organizing the buffer to calculate the CRC for it.
            OrganizedBuffer = organizedMesssageList.ToArray();
            CRC = CRC32B(OrganizedBuffer);
            return CRC;
        }

        // Get the whole buffer.
        public byte[] GetAll()
        {
            return AllMessage;
        }

        // Routin for ordering the buffer to calculate the CRC for it.
        private List<byte> Organize(List<byte> MesssageList) 
        {
            List<byte> OrganizedBuffer = new List<byte>();

            int MultiplesOf4 = 0;

            List<byte> temp = new List<byte>();
            foreach (byte item in MesssageList)
            {
                temp.Add(item);
                if (temp.Count == 4)
                {
                    MultiplesOf4++;
                    temp.Reverse();
                    foreach (byte itemReversed in temp)
                    {
                        OrganizedBuffer.Add(itemReversed);
                    }
                    temp.Clear();
                }
            }
            temp.Clear();
            if ((MesssageList.Count - OrganizedBuffer.Count) != 0)
            {
                int startingItem = MultiplesOf4 * 4;
                for (int i = startingItem; i < MesssageList.Count; i++)
                {
                    temp.Add(MesssageList[i]);
                }

                while (temp.Count < 4)
                    temp.Add(0);
                temp.Reverse();

                foreach (byte value in temp)
                {
                    OrganizedBuffer.Add(value);
                }
            }
            return OrganizedBuffer;
        }

        // Algorithm used in the Hexabitz modules hardware to calculate the correct CRC32 but we are only using the first byte in our modules. 
        private byte CRC32B(byte[] Buffer)  // Change byte to int to get the whole CRC32.
        {
            byte L = (byte)Buffer.Length;
            byte I, J;
            uint CRC, MSB;
            CRC = 0xFFFFFFFF;
            for (I = 0; I < L; I++)
            {
                CRC ^= (((uint)Buffer[I]) << 24);
                for (J = 0; J < 8; J++)
                {
                    MSB = CRC >> 31;
                    CRC <<= 1;
                    CRC ^= (0 - MSB) & 0x04C11DB7;
                }
            }
            return (byte)CRC; // Remove (byte) to get get the full int.
        }
    }
}
