# Continuous Integration & Delivery Strategy

## Purpose

This document defines the Continuous Integration and Continuous Delivery strategy adopted by the project.

The objective is to ensure that every change is validated, reproducible and deployable through an automated pipeline.

---

# Goals

* Automated validation
* Reproducible builds
* Deployment confidence
* Early defect detection
* Consistent delivery process
* Cloud deployment readiness

---

# Principles

## Principle 1

Every change must be validated automatically.

Manual validation is not sufficient.

---

## Principle 2

Main branch must always be deployable.

At any point in time:

```text
main
```

must represent a production-ready state.

---

## Principle 3

All deployments originate from source control.

No manual code changes are allowed in deployed environments.

---

## Principle 4

The pipeline is the single source of truth.

A deployment is considered valid only if it has passed through the CI/CD pipeline.

---

# Branch Strategy

The repository follows a trunk-based workflow.

Main branch:

```text
main
```

Feature branches:

```text
feature/create-order

feature/order-status

feature/order-validation
```

Bug fixes:

```text
fix/duplicate-order

fix/database-timeout
```

Hotfixes:

```text
hotfix/critical-production-fix
```

---

# Pull Request Workflow

All changes must be introduced through Pull Requests.

Workflow:

```text
Feature Branch
       │
       ▼
Pull Request
       │
       ▼
CI Validation
       │
       ▼
Code Review
       │
       ▼
Merge
```

Direct commits to main are not allowed.

---

# Branch Protection Rules

The main branch must be protected.

Required rules:

* Pull Request required
* Status checks required
* Branch up-to-date before merge
* Force push disabled

---

# Continuous Integration

Continuous Integration is executed on:

```text
Pull Request
```

and

```text
Push to main
```

---

# CI Responsibilities

The CI pipeline validates:

```text
Restore
Build
Tests
Docker Build
```

---

# CI Workflow

```text
Checkout
     │
     ▼

Restore
     │
     ▼

Build
     │
     ▼

Tests
     │
     ▼

Docker Build
```

If any step fails:

```text
Pipeline Failed
```

and the Pull Request cannot be merged.

---

# Build Validation

Every Pull Request must verify:

* Solution restores successfully
* Solution builds successfully
* Unit tests pass
* Docker image builds successfully

---

# Docker Validation

Every deployable service must successfully build its container image.

Validation includes:

```text
Docker Build
```

for the target service.

A service that cannot be containerized is not deployable.

---

# Continuous Delivery

Continuous Delivery is executed after:

```text
Merge to main
```

---

# CD Responsibilities

The delivery pipeline performs:

```text
Build
Docker Build
Docker Push
Deployment
```

---

# Deployment Flow

```text
Merge to Main
        │
        ▼

Build
        │
        ▼

Docker Build
        │
        ▼

Push Image
        │
        ▼

Deploy
```

---

# Container Registry

Docker images are published to:

```text
Amazon ECR
```

Image naming convention:

```text
orderflow/order-service
```

Version example:

```text
1.0.0
1.1.0
2.0.0
```

The latest tag must not be used for deployments.

---

# Deployment Target

Current deployment target:

```text
Amazon ECS Fargate
```

Future deployment targets may be introduced without changing the CI strategy.

---

# Secrets Management

Secrets are stored outside source control.

Allowed locations:

```text
GitHub Actions Secrets
AWS Secrets Manager
AWS Systems Manager Parameter Store
```

Forbidden:

```text
Source Code
Dockerfile
appsettings.json
```

for production credentials.

---

# Required Secrets

Examples:

```text
AWS_ACCESS_KEY_ID

AWS_SECRET_ACCESS_KEY

AWS_REGION

AWS_ACCOUNT_ID
```

Additional secrets may be introduced as infrastructure evolves.

---

# Versioning

The project follows Semantic Versioning.

Format:

```text
MAJOR.MINOR.PATCH
```

Examples:

```text
1.0.0
1.1.0
1.1.1
2.0.0
```

---

# Release Process

Release workflow:

```text
Feature
   │
   ▼

Pull Request
   │
   ▼

Merge
   │
   ▼

Deployment
   │
   ▼

Release Tag
```

Example:

```text
v1.0.0
```

---

# Deployment Readiness Checklist

A change is deployable only if:

* Build succeeds
* Tests pass
* Docker image builds successfully
* Branch protection requirements are satisfied
* No secrets are committed
* CI pipeline succeeds

---

# Initial Implementation Scope

Phase 1:

```text
CI Pipeline
```

Validation:

```text
Restore
Build
Tests
Docker Build
```

---

Phase 2:

```text
CD Pipeline
```

Deployment:

```text
GitHub Actions
        │
        ▼
Amazon ECR
        │
        ▼
Amazon ECS Fargate
```

---

# Future Enhancements

Potential future additions:

```text
Integration Tests

Security Scanning

Dependency Scanning

Container Vulnerability Scanning

Automated Release Generation
```

These capabilities are intentionally postponed until the base CI/CD workflow is stable.
