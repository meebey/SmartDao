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
    public class TooMuchDataException : SmartDaoException
    {
        public TooMuchDataException()
        {
        }
        
        public TooMuchDataException(string message) : base(message)
        {
        }
        
        protected TooMuchDataException(SerializationInfo si, StreamingContext sctx) : base(si, sctx)
        {
        }
    }
}
