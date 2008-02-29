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
    public class TooMuchDataUpdatedException : TooMuchDataException
    {
        public TooMuchDataUpdatedException()
        {
        }
        
        public TooMuchDataUpdatedException(string message) : base(message)
        {
        }
        
        protected TooMuchDataUpdatedException(SerializationInfo si, StreamingContext sctx) : base(si, sctx)
        {
        }
    }
}
