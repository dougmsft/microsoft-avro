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

namespace Microsoft.Hadoop.Avro.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Xunit;

    [Trait("Category","TypeExtensions")]
    public class TypeExtensionsTest
    {
        [Fact]
        public void CanBeKnownTypeOfTest()
        {
            // Object itself could not be known type of any type
            Assert.False(typeof(object).CanBeKnownTypeOf(typeof(int)));
            Assert.True(typeof(int).CanBeKnownTypeOf(typeof(object)));
            Assert.False(typeof(object).CanBeKnownTypeOf(typeof(object)));

            Assert.True(typeof(List<int>).CanBeKnownTypeOf(typeof(IEnumerable<int>)));
            Assert.True(typeof(Collection<int>).CanBeKnownTypeOf(typeof(IEnumerable<int>)));
            Assert.True(typeof(int[]).CanBeKnownTypeOf(typeof(IEnumerable<int>)));

            Assert.False(typeof(IEnumerable<int>).CanBeKnownTypeOf(typeof(IEnumerable<int>)));
            Assert.False(typeof(IEnumerable<int>).CanBeKnownTypeOf(typeof(IEnumerable<string>)));
            Assert.False(typeof(IEnumerable<int>).CanBeKnownTypeOf(typeof(IEnumerable<Guid>)));
            Assert.False(typeof(IEnumerable<int>).CanBeKnownTypeOf(typeof(IEnumerable<IListClass>)));
            Assert.False(typeof(IList<Guid>).CanBeKnownTypeOf(typeof(int[])));
            Assert.False(typeof(int).CanBeKnownTypeOf(typeof(Guid)));
            Assert.False(typeof(IList<int>).CanBeKnownTypeOf(typeof(IList<Guid>)));

            Assert.False(typeof(int[]).CanBeKnownTypeOf(typeof(IList<IListClass>)));
            Assert.False(typeof(int[]).CanBeKnownTypeOf(typeof(IList<Guid>)));
        }

        [Fact]
        public void GenericIsAssignableTest()
        {
            Assert.False(typeof(int[]).GenericIsAssignable(typeof(IDictionary<int, Guid>)));
            Assert.False(typeof(IList<int>).GenericIsAssignable(typeof(IDictionary<int, Guid>)));
            Assert.False(typeof(IDictionary<int, Guid>).GenericIsAssignable(typeof(IList<int>)));
        }
    }
}
