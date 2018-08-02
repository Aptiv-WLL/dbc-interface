using Aptiv.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aptiv.DBCFiles.Classes
{
    /// <summary>
    /// An individual signal from the data of the DBC Message.
    /// </summary>
    [Serializable]
    internal class DBCSignal : IDBCSignal
    {
        #region --- Properties ---

        /// <summary>
        /// The unique name of this signal.
        /// </summary>
        public string Name;
        /// <summary>
        /// The ECU parent of this Signal.
        /// </summary>
        public IDBCECU ECU;
        /// <summary>
        /// The Message parent of this signal.
        /// </summary>
        public DBCMessage Message;
        /// <summary>
        /// The function of this signal.
        /// </summary>
        public string Function;
        /// <summary>
        /// The bit length of the message
        /// </summary>
        public uint BitLength { get; private set; }
        /// <summary>
        /// The order of the bytes in this signal.
        /// </summary>
        public ByteOrder Order;

        /// <summary>
        /// The default value of the signal
        /// </summary>
        public ulong DefaultValue { get; private set; }
        private ulong SNA;

        //These are already exposed
        private readonly double PhysMax;
        private readonly double PhysMin;

        /// <summary>
        /// The normal maximum
        /// </summary>
        public ulong NormMax { get; private set; }

        /// <summary>
        ///The normal minimum 
        /// </summary>
        public ulong NormMin { get; private set; }

        /// <summary>
        /// The delta between physical Values
        /// </summary>
        public double Factor { get; private set; }

        /// <summary>
        /// Offset of the physical value
        /// </summary>
        public double Offset { get; private set; }

        /// <summary>
        /// Units of the signal
        /// </summary>
        public string Units { get; private set; }

        private string line;
        private uint StartBit;
        private IntDescriptor Descriptor;

        /// <summary>
        /// Specific attributes from the DBC file.
        /// </summary>
        [NonSerialized]
        public Dictionary<string, string> Attributes = new Dictionary<string, string>();

        /// <summary>
        /// Specified values from the DBC.
        /// </summary>
        [NonSerialized]
        public Dictionary<long, string> SpecifiedValues = new Dictionary<long, string>();

        #endregion

        #region --- Constructor ---

        /// <summary>
        /// Constuct a new DBC Signal.
        /// </summary>
        /// <param name="ecu">The parent ecu object.</param>
        /// <param name="message">The parent message object.</param>
        /// <param name="startbit">The bit index in the start byte at which this signal begins.</param>
        /// <param name="name">The name of this signal.</param>
        /// <param name="length">The number of bits in this signal.</param>
        /// <param name="def"></param>
        /// <param name="sna"></param>
        /// <param name="physMax">The physical maximum value of this signal.</param>
        /// <param name="physMin">The physical minimum value of this signal.</param>
        /// <param name="normMax"></param>
        /// <param name="normMin"></param>
        /// <param name="res"></param>
        /// <param name="units">The particular units of this signal's value.</param>
        /// <param name="descriptor"></param>
        /// <param name="order">The byteOrder of this signal (for multi-byte signals).</param>
        /// <param name="offset">The offset of the physical value.</param>
        /// <param name="line">The line in the dbc file, for debugging reasons.</param>
        public DBCSignal(DBCECU ecu, DBCMessage message, uint startbit, string name, uint length,
            ulong def, ulong sna, double physMax, double physMin, ulong normMax, ulong normMin, double res, string units,
            IntDescriptor descriptor, ByteOrder order, double offset, string line)
        {
            Order = order;
            ECU = ecu;
            Message = message;
            this.line = line;
            StartBit = startbit;

            Name = name;
            BitLength = length;

            DefaultValue = def;
            SNA = sna;

            // Range is specified
            PhysMax = physMax;
            PhysMin = physMin;
            NormMax = normMax;
            NormMin = normMin;
            Factor = res;
            Value = DefaultValue;
            Descriptor = descriptor;
            Offset = offset;
            Units = units;
        }

        /// <summary>
        /// Performs a deep copy of another DBCSignal.
        /// </summary>
        /// <param name="other">DBCSignal to copy.</param>
        public DBCSignal(DBCSignal other)
        {
            Order = other.Order;
            ECU = other.ECU;
            Message = other.Message;
            StartBit = other.StartBit;

            Name = other.Name;
            BitLength = other.BitLength;

            DefaultValue = other.DefaultValue;
            SNA = other.SNA;

            // Range is specified
            PhysMax = other.PhysicalMax;
            PhysMin = other.PhysicalMin;
            NormMax = other.NormMax;
            NormMin = other.NormMin;
            Factor = other.Factor;
            Descriptor = other.Descriptor;
            Offset = other.Offset;
            Units = other.Units;

            // Attributes
            foreach (var attr in other.Attributes)
                Attributes.Add(attr.Key, attr.Value);

            // Specific Values
            foreach (var spv in other.SpecifiedValues)
                SpecifiedValues.Add(spv.Key, spv.Value);

            Value = other.Value;
        }

        /// <summary>
        /// Create a deep copy of this Signal.
        /// </summary>
        /// <returns></returns>
        public DBCSignal Clone()
        {
            return new DBCSignal(this);
        }

        #endregion

        #region --- Value Parsers ---

        /// <summary>
        /// Obtain or set the value of this signal in the message.
        /// </summary>
        public ulong Value
        {
            get
            {
                IList<byte> DataBytes = Message.Data;

                uint Bi = StartBit / 8;
                uint bi = StartBit % 8;
                uint bRem = BitLength;

                ulong value = 0;

                while (bRem > 0)
                {
                    // DataBytes - The array of the bytes of data
                    // startbit  - The index of the lsb of the signal in the bytes
                    // bitLength - The number of bits composing the message



                    // number of bits in this Bi
                    int len = (int)(bRem < 8 - bi ? bRem : 8 - bi);

                    // create a mask for this byte index (Bi)
                    byte mask = (byte)(((byte)(0xFF << (8 - len))) >> (byte)(8 - bi - len));

                    // Mask the byte and shift it completely right
                    ulong newVal = (ulong)((DataBytes[(int)Bi] & mask) >> (int)bi);

                    // Shift the new value.
                    newVal <<= (int)(BitLength - bRem);

                    // Add the result
                    value += newVal;

                    // Update the Byte index.
                    if (Order == ByteOrder.Motorola) Bi--;
                    else Bi++;
                    bi = 0; // bit index gets set to 0 after the first iteration.
                    bRem -= (uint)len; // Track how many bits are left
                }

                return value;
            }
            set
            {
                // Copy over the data to avoid multi-accessing
                ulong val = value;
                IList<byte> DataBytes = Message.Data;

                // Initial values
                uint Bi = StartBit / 8;   // Byte index
                uint bi = StartBit % 8;   // bit  index
                uint bRem = BitLength;      // the number of bits left to work with

                while (bRem > 0)
                {
                    // number of bits in this Bi
                    int len = (int)(bRem < 8 - bi ? bRem : 8 - bi);

                    // create a mask for this byte index (Bi)
                    byte mask = (byte)(((byte)(0xFF << (8 - len))) >> (byte)(8 - bi - len));

                    // set the old byte to 0s where the new value will go
                    byte oldB = (byte)(DataBytes[(int)Bi] & ~mask);

                    // get the part of the new value for this byte
                    byte noLeadingBits = (byte)(value >> (int)(BitLength - bRem));
                    byte nicelyPlaced = (byte)(noLeadingBits << (int)bi);
                    byte newB = (byte)(nicelyPlaced & mask);

                    // join (or) the two together
                    newB = (byte)(oldB | newB);

                    // reassign
                    DataBytes[(int)Bi] = newB;

                    // update values and loop
                    if (Order == ByteOrder.Motorola) Bi--;
                    else Bi++;
                    bi = 0; // bit index gets set to 0 after the first iteration.
                    bRem -= (uint)len; // finished up with 'len' bits.
                }
            }
        }

        /// <summary>
        /// The physical value represented in this signal.
        /// </summary>
        public double PhysicalValue
        {
            get
            {
                return ConvertToPhysical(Value);
            }
            set
            {
                Value = ConvertToSignalValue(value);
            }
        }

        /// <summary>
        /// The maximum value of the Value.
        /// </summary>
        public ulong MaxValue { get => ((ulong)1 << (int)(BitLength)); }

        /// <summary>
        /// The minimum value of the Value.
        /// </summary>
        public ulong MinValue { get => 0; }

        /// <summary>
        /// The maximum value of the PhysicalValue.
        /// </summary>
        public double PhysicalMax { get => MaxValue * Factor + Offset; }

        /// <summary>
        /// The minimum value of the PhysicalValue.
        /// </summary>
        public double PhysicalMin { get => MinValue * Factor + Offset; }

        /// <summary>
        /// Converts the signal value to the physical value
        /// </summary>
        /// <param name="value">Signal value</param>
        /// <returns>The physical value of the signal</returns>
        public double ConvertToPhysical(ulong value)
        {
            return value * Factor + Offset;
        }

        /// <summary>
        /// Converts the physicalValue to the Signal value
        /// </summary>
        /// <param name="physicalValue">The physcial value</param>
        /// <returns>the value of the signal</returns>
        public ulong ConvertToSignalValue(double physicalValue)
        {
            return (ulong)((physicalValue - Offset) / Factor);
        }

        #endregion

        #region --- IDBCSignal Interface ---

        string IDBCSignal.Name => Name;

        IDBCECU IDBCSignal.ECU => ECU;

        IDBCMessage IDBCSignal.Message => Message;

        string IDBCSignal.Function => Function;

        ByteOrder IDBCSignal.Order => Order;

        IReadOnlyDictionary<string, string> IDBCSignal.Attributes => Attributes;

        IReadOnlyDictionary<long, string> IDBCSignal.SpecifiedValues => SpecifiedValues;

        uint IDBCSignal.StartBit => StartBit;

        ulong IDBCSignal.SNA => SNA;

        IntDescriptor IDBCSignal.Descriptor => Descriptor;

        #endregion
    }
}
