﻿using System;

namespace WebSample.Models
{
    public class ProtectionData
    {
        public string PlainText { get; set; }
        public string CipherText { get; set; }
        public Exception Error { get; set; }
    }
}
