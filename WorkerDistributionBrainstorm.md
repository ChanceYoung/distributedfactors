## Considerations for deciding how to schedule workers

In this version of the project as of 4/1/2022, there is a dynamic "round robin" style which occurs on the server so that every time a request to the server is receiving containing the reserved "RTR" message, a worker gets added to the internal list of workers. Then when a request is received, a counter increments and we choose the next worker based on the `request_count % list.length`. 

This is good, but what if a worker takes a long time? we wont know that its stuck, and then our whole process will get stuck. So we could do a couple of things. First, we need a way to make health checks to our workers in another thread, and then we could implement a Circuit Breaker or Bulkhead pattern so that the worker could be skipped over or failed fast. We could also set up a way to add more workers to the list if all our workers are busy, but this would get expensive quickly. 