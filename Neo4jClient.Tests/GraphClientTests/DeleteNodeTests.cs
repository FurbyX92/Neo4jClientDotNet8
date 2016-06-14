﻿using System;
using System.Net;
using NUnit.Framework;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class DeleteNodeTests
    {
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            client.Delete(123, DeleteMode.NodeOnly);
        }

        [Test]
        public void ShouldDeleteNodeOnly()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Delete("/node/456"),
                    MockResponse.Http(204)
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();
                graphClient.Delete(456, DeleteMode.NodeOnly);
            }
        }

        [Test]
        public void ShouldDeleteAllRelationshipsFirst()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get("/node/456/relationships/all"),
                    MockResponse.Json(HttpStatusCode.OK,
                        @"[
                          { 'self': 'http://foo/db/data/relationship/56',
                            'start': 'http://foo/db/data/node/123',
                            'end': 'http://foo/db/data/node/456',
                            'type': 'KNOWS',
                            'properties': 'http://foo/db/data/relationship/56/properties',
                            'property': 'http://foo/db/data/relationship/56/properties/{key}',
                            'data': { 'date': 1270559208258 }
                          },
                          { 'self': 'http://foo/db/data/relationship/78',
                            'start': 'http://foo/db/data/node/456',
                            'end': 'http://foo/db/data/node/789',
                            'type': 'KNOWS',
                            'properties': 'http://foo/db/data/relationship/78/properties',
                            'property': 'http://foo/db/data/relationship/78/properties/{key}',
                            'data': { 'date': 1270559208258 }
                          }
                        ]")
                },
                {
                    MockRequest.Delete("/relationship/56"),
                    MockResponse.Http(204)
                },
                {
                    MockRequest.Delete("/relationship/78"),
                    MockResponse.Http(204)
                },
                {
                    MockRequest.Delete("/node/456"),
                    MockResponse.Http(204)
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();
                graphClient.Delete(456, DeleteMode.NodeAndRelationships);
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Unable to delete the node. The node may still have relationships. The response status was: 409 Conflict")]
        public void ShouldThrowExceptionWhenDeleteFails()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Delete("/node/456"),
                    MockResponse.Http(409)
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();
                graphClient.Delete(456, DeleteMode.NodeOnly);
            }
        }
    }
}
