locals {
  default_tags = {
    "env" : var.environment,
    "app" : "test-distributed-tracing"
  }
}