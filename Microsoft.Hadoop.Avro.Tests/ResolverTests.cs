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
    using System.Linq;
    using System.Runtime.Serialization;
    using Xunit;

    [Trait("Category","Resolver")]
    public sealed class ResolverTests
    {
        [Fact]
        public void Resolver_GetSerializationInfoUsingDataContactResolverWithNullFieldInfo()
        {
            Assert.Throws<ArgumentNullException>(() =>
                {
                    var resolver = new AvroDataContractResolver();
                    resolver.ResolveMembers(null);
                }
            );
        }

        [Fact]
        public void Resolver_GetSerializationInfoDataContractResolverWithUnsupportedTypes()
        {
            Assert.Throws<SerializationException>(() =>
                {
                    var resolver = new AvroDataContractResolver();
                    resolver.ResolveType(typeof(ClassWithoutParameterlessConstructor));
                }
            );
        }

        [Fact]
        public void Resolver_GetSerializationInfoUsingPublicMembersResolverWithNullFieldInfo()
        {
            Assert.Throws<ArgumentNullException>(() =>
                {
                    var resolver = new AvroPublicMemberContractResolver();
                    resolver.ResolveMembers(null);
                }
            );
        }

        [Fact]
        public void Resolver_GetSerializationInfoPublicMembersResolverWithUnsupportedTypes()
        {
            Assert.Throws<SerializationException>(() =>
                {
                    var resolver = new AvroPublicMemberContractResolver();
                    resolver.ResolveType(typeof(ClassWithoutParameterlessConstructor));
                }
            );
        }

        [Fact]
        public void Resolver_GetKnownTypesUsingDataContactResolverForAbstractClassAndInvalidTypes()
        {
            var resolver = new AvroDataContractResolver();
            var knownTypes = resolver.GetKnownTypes(typeof(AbstractClassWithInvalidKnownTypes));
            Assert.True(knownTypes.SequenceEqual(new[] { typeof(Rectangle) }));
        }

        [Fact]
        public void Resolver_GetKnownTypesUsingDataContractResolverWithNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                {
                    var resolver = new AvroDataContractResolver();
                    resolver.GetKnownTypes(null);
                }
            );
        }

        [Fact]
        public void Resolver_GetKnownTypesUsingPublicMembersResolverWithNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                {
                    var resolver = new AvroPublicMemberContractResolver();
                    resolver.GetKnownTypes(null);
                }
            );
        }

        [Fact]
        public void Resolver_GetKnownTypesUsingDataContactResolverForAbstractClassAndValidTypes()
        {
            var resolver = new AvroDataContractResolver();
            var knownTypes = resolver.GetKnownTypes(typeof(AbstractShape)).ToList();

            Assert.Equal(2, knownTypes.Count);
            Assert.True(
                (knownTypes[0] == typeof(Square) && knownTypes[1] == typeof(Rectangle)) ||
                (knownTypes[1] == typeof(Square) && knownTypes[0] == typeof(Rectangle)));
        }

        [Fact]
        public void Resolver_GetSerializationInfoUsingDataContactResolverWithNullType()
        {
            Assert.Throws<ArgumentNullException>(() =>
                {
                    var resolver = new AvroDataContractResolver();
                    resolver.ResolveType(null);
                }
            );
        }

        [Fact]
        public void Resolver_GetSerializationInfoUsingPublicMembersResolverWithNullType()
        {
            Assert.Throws<ArgumentNullException>(() =>
                {
                    var resolver = new AvroPublicMemberContractResolver();
                    resolver.ResolveType(null);
                }
            );
        }

        [Fact]
        public void Resolver_DataContractResolverEquality()
        {
            var resolver = new AvroDataContractResolver();
            var secondResolver = new AvroDataContractResolver();
            var thirdResolver = new AvroDataContractResolver();

            Utilities.VerifyEquality(resolver, secondResolver, thirdResolver);
        }

        [Fact]
        public void Resolver_PublicMembersResolverEquality()
        {
            var resolver = new AvroPublicMemberContractResolver();
            var secondResolver = new AvroPublicMemberContractResolver();
            var thirdResolver = new AvroPublicMemberContractResolver();

            Utilities.VerifyEquality(resolver, secondResolver, thirdResolver);
        }

    }
}
