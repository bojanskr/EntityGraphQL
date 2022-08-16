﻿using Xunit;
using EntityGraphQL.Schema;
using System;
using System.Linq;

namespace EntityGraphQL.Tests
{
    public class MutationMethodParameterTests
    {
        [Fact]
        public void TestSeparateArguments_PrimitivesOnly()
        {
            var schemaProvider = SchemaBuilder.FromObject<TestDataContext>();
            schemaProvider.AddScalarType<DateTime>("DateTime", "");
            schemaProvider.AddScalarType<decimal>("decimal", "");
            schemaProvider.AddMutationsFrom<PeopleMutations>(false);
            // Add a argument field with a require parameter
            var gql = new QueryRequest
            {
                Query = @"mutation addPersonPrimitive($id: Int, $name: String!, $birthday: DateTime, $weight: decimal, $gender: Gender) {
                  addPersonPrimitive(id: $id, name: $name, birthday: $birthday, weight: $weight, gender: $gender) { id name }
                }",
                Variables = new QueryVariables {
                    { "id", 3 },
                    { "name", "Frank" },
                    { "birthday", DateTime.Today },
                    { "weight", 45.5 },
                    { "gender", Gender.Male }
                }
            };
            var res = schemaProvider.ExecuteRequest(gql, new TestDataContext(), null, null);
            Assert.Null(res.Errors);
        }

        [Fact]
        public void TestSeparateArguments_PrimitivesOnly_WithInlineDefaults()
        {
            var schemaProvider = SchemaBuilder.FromObject<TestDataContext>();
            schemaProvider.AddScalarType<DateTime>("DateTime", "");
            schemaProvider.AddScalarType<decimal>("decimal", "");
            schemaProvider.AddMutationsFrom<PeopleMutations>(false);
            // Add a argument field with a require parameter
            var gql = new QueryRequest
            {
                Query = @"mutation addPersonPrimitive($birthday: DateTime, $weight: decimal, $gender: Gender) {
                  addPersonPrimitive(id: 3, name: """", birthday: $birthday, weight: $weight, gender: $gender) { id name }
                }",
                Variables = new QueryVariables {
                    { "birthday", DateTime.Today },
                    { "weight", 45.5 },
                    { "gender", Gender.Male }
                }
            };
            var res = schemaProvider.ExecuteRequest(gql, new TestDataContext(), null, null);
            Assert.Null(res.Errors);
        }

        [Fact]
        public void TestSeparateArguments()
        {
            var schemaProvider = SchemaBuilder.FromObject<TestDataContext>();
            schemaProvider.AddInputType<InputObject>("InputObject", "");
            schemaProvider.AddMutationsFrom<PeopleMutations>(false);
            // Add a argument field with a require parameter
            var gql = new QueryRequest
            {
                Query = @"mutation AddPersonSeparateArguments($name: String!, $names: [String!], $nameInput: InputObject, $gender: Gender) {
                  addPersonSeparateArguments(name: $name, names: $names, nameInput: $nameInput, gender: $gender) { id name }
                }",
                Variables = new QueryVariables {
                    { "name", "Frank" },
                    { "names", new [] { "Frank" } },
                    { "nameInput", null },
                    { "gender", Gender.Female }
                }
            };
            var res = schemaProvider.ExecuteRequest(gql, new TestDataContext(), null, null);
            Assert.Null(res.Errors);
        }

        [Fact]
        public void TestSingleArgument()
        {
            var schemaProvider = SchemaBuilder.FromObject<TestDataContext>();
            schemaProvider.AddInputType<InputObject>("InputObject", "");
            schemaProvider.AddMutationsFrom<PeopleMutations>(false);
            // Add a argument field with a require parameter
            var gql = new QueryRequest
            {
                Query = @"mutation AddPersonSingleArgument($nameInput: InputObject) {
                  addPersonSingleArgument(nameInput: $nameInput) { id name }
                }",
                Variables = new QueryVariables {
                    { "nameInput", new InputObject() { Name = "Frank" } },
                }
            };
            var res = schemaProvider.ExecuteRequest(gql, new TestDataContext(), null, null);
            Assert.Null(res.Errors);
        }

        [Fact]
        public void TestSingleArgument_AutoAddInputTypes()
        {
            var schemaProvider = SchemaBuilder.FromObject<TestDataContext>();
            schemaProvider.AddMutationsFrom<PeopleMutations>(true);
            // Add a argument field with a require parameter
            var gql = new QueryRequest
            {
                Query = @"mutation AddPersonSingleArgument($nameInput: InputObject) {
                  addPersonSingleArgument(nameInput: $nameInput) { id name }
                }",
                Variables = new QueryVariables {
                    { "nameInput", new InputObject() { Name = "Frank" } },
                }
            };
            var res = schemaProvider.ExecuteRequest(gql, new TestDataContext(), null, null);
            Assert.Null(res.Errors);
        }

        [Fact]
        public void TestSingleArgument_AutoAddInputTypes_NullableNestedType()
        {
            var schemaProvider = SchemaBuilder.FromObject<TestDataContext>();
            schemaProvider.AddMutationsFrom<PeopleMutations>(true);
            var schema = schemaProvider.ToGraphQLSchemaString();

            Assert.Contains("addPersonNullableNestedType(required: NestedInputObject!, optional: NestedInputObject): Person!", schema);
        }

        [Fact]
        public void TestSeparateArguments_AutoAddInputTypes()
        {
            var schemaProvider = SchemaBuilder.FromObject<TestDataContext>();
            schemaProvider.AddMutationsFrom<PeopleMutations>(true);
            // Add a argument field with a require parameter
            var gql = new QueryRequest
            {
                Query = @"mutation AddPersonSeparateArguments($name: String!, $names: [String!], $nameInput: InputObject, $gender: Gender) {
                  addPersonSeparateArguments(name: $name, names: $names, nameInput: $nameInput, gender: $gender) { id name }
                }",
                Variables = new QueryVariables {
                    { "name", "Frank" },
                    { "names", new [] { "Frank" } },
                    { "nameInput", null },
                    { "gender", Gender.Female }
                }
            };
            var res = schemaProvider.ExecuteRequest(gql, new TestDataContext(), null, null);
            Assert.Null(res.Errors);
        }

        [Fact]
        public void TestChildArraysDontGetArguments()
        {
            var schemaProvider = new SchemaProvider<TestDataContext>();
            schemaProvider.AddScalarType<DateTime>("DateTime", "");
            schemaProvider.AddScalarType<decimal>("decimal", "");
            schemaProvider.AddScalarType<char>("char", "");
            schemaProvider.PopulateFromContext();
            schemaProvider.AddInputType<ListOfObjectsWithIds>("ListOfObjectsWithIds", "").AddAllFields();

            schemaProvider.AddMutationsFrom<PeopleMutations>(true);

            Assert.Empty(schemaProvider.GetSchemaType("ListOfObjectsWithIds", null).GetFields().Where(x => x.Arguments.Any()));
        }
    }
}
