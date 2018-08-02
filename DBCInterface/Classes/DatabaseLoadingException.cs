using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Aptiv.DBCFiles
{
    /// <summary>
    /// A class to handle Exceptions specific to loading a database file.
    /// </summary>
    [Serializable]
    public class DatabaseLoadingException : Exception, ISerializable
    {
        private readonly DBCFile partialDatabase;
        string fileLine;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fileLine"></param>
        public DatabaseLoadingException(DBCFile file, string fileLine = "")
        {
            partialDatabase = file;
            this.fileLine = fileLine;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="file"></param>
        /// <param name="fileLine"></param>
        public DatabaseLoadingException(string message, DBCFile file, string fileLine = "")
            : base(message)
        {
            partialDatabase = file;
            this.fileLine = fileLine;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        /// <param name="file"></param>
        /// <param name="fileLine"></param>
        public DatabaseLoadingException(string message, Exception inner, DBCFile file, string fileLine = "")
            : base(message, inner)
        {
            partialDatabase = file;
            this.fileLine = fileLine;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Partial Database", partialDatabase);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            GetObjectData(info, context);
        }
    }
}
