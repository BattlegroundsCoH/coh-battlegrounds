﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.ErrorHandling {
    
    public class EnvironmentException : Exception {
        public EnvironmentException(string message) : base(message) { }
    }

}