
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
// permissions and limitations under the License.

namespace Microsoft.Hadoop.Avro.Utils
{
    using System;
    using System.Collections;

    class CompilerError
    {
        private string errorText;
        private bool   isWarning;
        private IList  errorList;

        public CompilerError()
        {
            errorList = new IList();
        }

        public string ErrorText
        {
            get { return errorText; }
            set { errorText = value; }
        }

        public bool IsWarning
        {
            get { return isWarning; }
            set { isWarning = value; }
        }

    }

    class CompilerErrorCollection : System.Collections.ICollection
    {
        int ICollection.Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int Add(CompilerError value)
        {
            return errorList.Add(value);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}