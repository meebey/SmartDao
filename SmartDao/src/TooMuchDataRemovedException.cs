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
    public class TooMuchDataRemovedException : TooMuchDataException
    {
        public TooMuchDataRemovedException()
        {
        }
        
        public TooMuchDataRemovedException(string message) : base(message)
        {
        }
        
        protected TooMuchDataRemovedException(SerializationInfo si, StreamingContext sctx) : base(si, sctx)
        {
        }
    }
}
