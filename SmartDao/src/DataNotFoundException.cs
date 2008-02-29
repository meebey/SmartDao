/*
 * $Id$
 * $URL$
 * $Revision$
 * $Author$
 * $Date$
 */

using System;
using System.Runtime.Serialization;

namespace Meebey.SmartDao
{
    [Serializable]
    public class DataNotFoundException : SmartDaoException
    {
        public DataNotFoundException()
        {
        }
        
        public DataNotFoundException(string message) : base(message)
        {
        }
        
        protected DataNotFoundException(SerializationInfo si, StreamingContext sctx) : base(si, sctx)
        {
        }
    }
}
