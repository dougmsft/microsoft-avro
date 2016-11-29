// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not
// use this file except in compliance with the License.  You may obtain a copy
// of the License at http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED
// WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
// MERCHANTABLITY OR NON-INFRINGEMENT.
// 
// See the Apache Version 2.0 License for specific language governing
// permissions and limitations under the License

namespace AvroTestApp
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Hadoop.Avro.Container;

    [DataContract]
    internal struct TestMsg
    {
        [DataMember]
        public int Id;
        [DataMember]
        public double Amount;
        public TestMsg(int id, double amount)
        {
            Id = id;
            Amount = amount;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            string line = Environment.NewLine;

            string fileName = "Messages.avro";
            string filePath = null;
            if (Environment.NewLine.Contains("\r"))
            {
                filePath = new DirectoryInfo(".") + @"\" + fileName;
            }
            else
            {
                filePath = new DirectoryInfo(".") + @"/" + fileName;
            }

            List<TestMsg> msgs = new List<TestMsg>();
            msgs.Add(new AvroTestApp.TestMsg(1, 189.12));
            msgs.Add(new AvroTestApp.TestMsg(2, 345.94));

            using (var dataStream = new FileStream(filePath, FileMode.Create))
            {
                using (var avroWriter = AvroContainer.CreateWriter<TestMsg>(dataStream, Codec.Deflate))
                {
                    using (var seqWriter = new SequentialWriter<TestMsg>(avroWriter, msgs.Count))
                    {
                        msgs.ForEach(seqWriter.Write);
                    }
                }
                dataStream.Dispose();
            }
        }
    }
}
