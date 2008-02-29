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
    public class SmartDaoException : ApplicationException
    {
        public SmartDaoException()
        {
        }
        
        public SmartDaoException(string message) : base(message)
        {
        }
        
        protected SmartDaoException(SerializationInfo si, StreamingContext sctx) : base(si, sctx)
        {
        }
    }
}
