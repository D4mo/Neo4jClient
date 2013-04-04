﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class StartBitFormatterTests
    {
        [Test]
        public void ThrowNotSupportedExceptionForUnknownType()
        {
            var badObject = new { n1 = new StartBitFormatterTests() };
            Assert.Throws<NotSupportedException>(
                () => StartBitFormatter.FormatAsCypherText(badObject, null)
            );
        }

        [Test]
        public void NotSupportedExceptionForUnknownTypeIncludesIdentityName()
        {
            var badObject = new { n1 = new StartBitFormatterTests() };
            var exception = Assert.Throws<NotSupportedException>(
                () => StartBitFormatter.FormatAsCypherText(badObject, null)
            );
            StringAssert.Contains("n1", exception.Message);
        }

        [Test]
        public void NotSupportedExceptionForUnknownTypeIncludesTypeName()
        {
            var badObject = new { n1 = new StartBitFormatterTests() };
            var exception = Assert.Throws<NotSupportedException>(
                () => StartBitFormatter.FormatAsCypherText(badObject, null)
            );
            StringAssert.Contains(typeof(StartBitFormatterTests).FullName, exception.Message);
        }

        [Test]
        public void SingleNodeByStaticReference()
        {
            // START n1=node(1)
            var cypher = ToCypher(new
            {
                n1 = (NodeReference) 1
            });

            Assert.AreEqual("n1=node({p0})", cypher.QueryText);
            Assert.AreEqual(1, cypher.QueryParameters.Count);
            Assert.AreEqual(1, cypher.QueryParameters["p0"]);
        }

        [Test]
        public void MultipleNodesByStaticReference()
        {
            // START n1=node(1)
            var cypher = ToCypher(new
            {
                n1 = (NodeReference)1,
                n2 = (NodeReference)2
            });

            Assert.AreEqual("n1=node({p0}), n2=node({p1})", cypher.QueryText);
            Assert.AreEqual(2, cypher.QueryParameters.Count);
            Assert.AreEqual(1, cypher.QueryParameters["p0"]);
            Assert.AreEqual(2, cypher.QueryParameters["p1"]);
        }

        [Test]
        public void CustomString()
        {
            var cypher = ToCypher(new
            {
                n1 = "foo"
            });

            Assert.AreEqual("n1=foo", cypher.QueryText);
            Assert.AreEqual(0, cypher.QueryParameters.Count);
        }

        static CypherQuery ToCypher(object startBits)
        {
            var parameters = new Dictionary<string, object>();
            Func<object, string> createParameter = value =>
            {
                var name = "p" + parameters.Count;
                parameters.Add(name, value);
                return string.Format("{{{0}}}", name);
            };

            var cypherText = StartBitFormatter.FormatAsCypherText(startBits, createParameter);

            var query = new CypherQuery(cypherText, parameters, CypherResultMode.Projection);
            return query;
        }
    }
}