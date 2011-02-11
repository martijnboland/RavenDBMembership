using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace RavenDBMembership.IntegrationTests
{
    public static class Unique
    {
        public static int Integer { get { return _integer++; }  }
        static int _integer = 0;
    }
}
