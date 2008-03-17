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
    public abstract class TooMuchDataException : SmartDaoException
    {
        protected TooMuchDataException()
        {
        }
        
        protected TooMuchDataException(string message) : base(message)
        {
        }
        
        protected TooMuchDataException(SerializationInfo si, StreamingContext sctx) : base(si, sctx)
        {
        }
    }
}
