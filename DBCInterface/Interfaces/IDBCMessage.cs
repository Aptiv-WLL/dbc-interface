using Aptiv.Messages.CAN;
using Aptiv.Messaging;
using System;
using System.Collections.Generic;

namespace Aptiv.DBCFiles
{
    /// <summary>
    /// A message object described by the DBC.
    /// </summary>
    public interface IDBCMessage : IMessage<byte>, ICloneable
    {
        /// <summary>
        /// The dictionary of Signals contained in this DBC message.
        /// </summary>
        IReadOnlyDictionary<string, IDBCSignal> Signals { get; }

        /// <summary>
        /// The unique name of this Message.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// A flag to indicate whether this message can be sent periodically.
        /// </summary>
        bool Cyclic { get; }

        /// <summary>
        /// The length of time in milliseconds to send this message periodically.
        /// </summary>
        uint CycleTime { get; }

        /// <summary>
        /// The containing ECU for this message.
        /// </summary>
        IDBCECU ECU { get; }

        /// <summary>
        /// Defines the way to send.
        /// </summary>
        string LaunchType { get; }

        /// <summary>
        /// The time to wait before sending this message.
        /// </summary>
        int DelayTime { get; }

        /// <summary>
        /// Updates this IDBCMessage with the data from the given CAN message.
        /// </summary>
        /// <param name="m">The data to use to update the signals of this
        /// DBC message.</param>
        /// <returns>True on success.</returns>
        bool Update(CANMessage m);
    }
}
