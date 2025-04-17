curl -X POST http://localhost:7071/runtime/webhooks/eventgrid\?functionName=FoundryRelayFunction \
  -H "Content-Type: application/json" \
  -d '[{
    "id": "1234",
    "eventType": "Microsoft.Storage.BlobCreated",
    "subject": "/blobServices/default/containers/test-container/blobs/test.txt",
    "eventTime": "2024-10-01T01:01:01.000Z",
    "data": {
      "api": "PutBlob",
      "clientRequestId": "abc123",
      "requestId": "xyz789"
    },
    "dataVersion": "",
    "metadataVersion": "1"
  }]'
