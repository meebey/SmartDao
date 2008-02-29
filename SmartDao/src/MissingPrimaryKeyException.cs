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
    public class MissingPrimaryKeyException : SmartDaoException
    {
        public MissingPrimaryKeyException()
        {
        }
        
        public MissingPrimaryKeyException(string message) : base(message)
        {
        }
        
        protected MissingPrimaryKeyException(SerializationInfo si, StreamingContext sctx) : base(si, sctx)
        {
        }
    }
}
