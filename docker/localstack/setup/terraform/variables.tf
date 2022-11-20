variable "environment" {
  description = "Target environment"
  type        = string
  default     = ""
}

variable "create_resources" {
  description = "Enable creation of resources in a given environment."
  type        = bool
  default     = true
}
