using Aptiv.Messaging;
using System.Collections.Generic;

namespace Aptiv.DBCFiles
{
    /// <summary>
    /// A signal object described by the DBC file.
    /// </summary>
    public interface IDBCSignal
    {
        /// <summary>
        /// The unique identifier for this signal.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The ECU parent of this Signal.
        /// </summary>
        IDBCECU ECU { get; }

        /// <summary>
        /// The Message parent of this signal.
        /// </summary>
        IDBCMessage Message { get; }

        /// <summary>
        /// The function of this signal.
        /// </summary>
        string Function { get; }

        /// <summary>
        /// The bit length of the message
        /// </summary>
        uint BitLength { get; }

        /// <summary>
        /// The default value of the signal
        /// </summary>
        ulong DefaultValue { get; }

        /// <summary>
        /// The normal maximum
        /// </summary>
        ulong NormMax { get; }

        /// <summary>
        ///The normal minimum 
        /// </summary>
        ulong NormMin { get; }

        /// <summary>
        /// The delta between physical Values
        /// </summary>
        double Factor { get; }

        /// <summary>
        /// Offset of the physical value
        /// </summary>
        double Offset { get; }

        /// <summary>
        /// Units of the signal
        /// </summary>
        string Units { get; }

        /// <summary>
        /// The order of the bytes in the signal.
        /// </summary>
        ByteOrder Order { get; }

        /// <summary>
        /// Specific attributes from the DBC file.
        /// </summary>
        IReadOnlyDictionary<string, string> Attributes { get; }

        /// <summary>
        /// Specified values from the DBC.
        /// </summary>
        IReadOnlyDictionary<long, string> SpecifiedValues { get; }

        /// <summary>
        /// Obtain or set the value of this signal in the message.
        /// </summary>
        ulong Value { get; set; }

        /// <summary>
        /// The physical value represented in this signal.
        /// </summary>
        double PhysicalValue { get; set; }

        /// <summary>
        /// The maximum value of the Value.
        /// </summary>
        ulong MaxValue { get; }

        /// <summary>
        /// The minimum value of the Value.
        /// </summary>
        ulong MinValue { get; }

        /// <summary>
        /// The maximum value of the PhysicalValue.
        /// </summary>
        double PhysicalMax { get; }

        /// <summary>
        /// The minimum value of the PhysicalValue"/>
        /// </summary>
        double PhysicalMin { get; }

        /// <summary>
        /// The starting location of this signal in a message.
        /// </summary>
        uint StartBit { get; }

        /// <summary>
        /// 
        /// </summary>
        ulong SNA { get; }

        /// <summary>
        /// 
        /// </summary>
        IntDescriptor Descriptor { get; }
    }
}
