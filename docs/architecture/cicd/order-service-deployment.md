# Order Service Deployment Strategy

## Purpose

This document defines how the Order Service container image is published and versioned.

The objective is to provide a repeatable and automated deployment process from source code to Amazon Elastic Container Registry (ECR).

This phase intentionally excludes runtime deployment to ECS.

---

# Scope

Current scope:

```text
GitHub
    │
    ▼

GitHub Actions
    │
    ▼

Docker Build
    │
    ▼

Amazon ECR
```

Out of scope:

```text
Amazon ECS

CloudWatch

Autoscaling

Blue/Green Deployments
```

These capabilities will be introduced in later phases.

---

# Goals

* Automated image publishing
* Reproducible deployments
* Immutable artifacts
* Version traceability
* Cloud readiness

---

# Deployment Architecture

```text
Developer
     │
     ▼

Git Push
     │
     ▼

GitHub Actions
     │
     ▼

Docker Build
     │
     ▼

Amazon ECR
```

---

# Deployment Trigger

The deployment workflow executes when:

```text
Push to main
```

Only validated code may be deployed.

All Pull Request validations must succeed before merge.

---

# Prerequisites

The following validations must already be completed:

```text
Build

Tests

Docker Validation
```

Deployment is executed only after successful CI validation.

---

# Container Registry

Target registry:

```text
Amazon Elastic Container Registry (ECR)
```

Repository:

```text
orderflow/order-service
```

---

# Image Versioning

Images must be immutable.

The deployment workflow must generate image tags using:

```text
Git Commit SHA
```

Example:

```text
6a92c4d
1f93d8b
7be4f61
```

This guarantees traceability between source code and deployed artifacts.

---

# Tagging Rules

Allowed:

```text
Git SHA
Semantic Version
Release Tag
```

Forbidden:

```text
latest
```

Production deployments must never depend on mutable tags.

---

# Deployment Workflow

```text
Checkout
    │
    ▼

Configure AWS Credentials
    │
    ▼

Authenticate to ECR
    │
    ▼

Build Docker Image
    │
    ▼

Tag Docker Image
    │
    ▼

Push Docker Image
```

---

# AWS Authentication

GitHub Actions authenticates using an IAM user.

Required credentials:

```text
AWS_ACCESS_KEY_ID

AWS_SECRET_ACCESS_KEY

AWS_REGION

AWS_ACCOUNT_ID
```

Credentials are stored as:

```text
GitHub Actions Secrets
```

Credentials must never be committed to source control.

---

# Repository Secrets

Required secrets:

```text
AWS_ACCESS_KEY_ID

AWS_SECRET_ACCESS_KEY

AWS_REGION

AWS_ACCOUNT_ID
```

Example:

```text
AWS_REGION=us-east-1
```

---

# Image Name

The workflow publishes the image to:

```text
{AWS_ACCOUNT_ID}.dkr.ecr.{AWS_REGION}.amazonaws.com/orderflow/order-service:{GIT_SHA}
```

Example:

```text
123456789012.dkr.ecr.us-east-1.amazonaws.com/orderflow/order-service:6a92c4d
```

The workflow must not publish or deploy:

```text
latest
```

---

# Build Requirements

Before publishing:

* Docker image builds successfully
* Solution builds successfully
* Tests pass
* Docker validation succeeds

---

# Workflow Location

GitHub Actions workflow:

```text
.github/workflows/deploy-order-service.yml
```

The workflow owns only Order Service image publication.

Runtime deployment to ECS remains out of scope for this phase.

---

# Failure Handling

Deployment must fail immediately if:

```text
AWS authentication fails

Docker build fails

Docker tagging fails

Docker push fails
```

Partial deployments are not allowed.

---

# Deployment Readiness Checklist

A deployment may execute only if:

* CI validation succeeded
* ECR repository exists
* AWS credentials are valid
* Docker image builds successfully
* GitHub Secrets are configured

---

# First Iteration Deliverable

The first deployment milestone is achieved when:

```text
Git Push
     │
     ▼

GitHub Actions
     │
     ▼

Amazon ECR
```

results in a new image appearing inside:

```text
orderflow/order-service
```

without any manual AWS CLI commands.

---

# Future Evolution

Phase 2:

```text
Amazon ECR
      │
      ▼

Amazon ECS Fargate
```

Phase 3:

```text
CloudWatch
```

Phase 4:

```text
Health Checks

Observability

Autoscaling
```

The current implementation stops at ECR publication.
