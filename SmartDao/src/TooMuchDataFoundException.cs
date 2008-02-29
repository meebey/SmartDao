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
    public class TooMuchDataFoundException : TooMuchDataException
    {
        public TooMuchDataFoundException()
        {
        }
        
        public TooMuchDataFoundException(string message) : base(message)
        {
        }
        
        protected TooMuchDataFoundException(SerializationInfo si, StreamingContext sctx) : base(si, sctx)
        {
        }
    }
}
