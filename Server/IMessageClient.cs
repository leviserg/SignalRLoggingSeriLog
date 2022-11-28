using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server
{
    public interface IMessageClient
    {
        Task Send(NewMessage message);
    }
}
