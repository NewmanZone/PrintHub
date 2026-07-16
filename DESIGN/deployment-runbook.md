# Deployment And Rollback Runbook

PrintHub deployments are manual, separately approved operations. Merging to `main`
does not authorize or start a deployment.

## Preconditions

- Identify the exact commit SHA and target environment (`dev`, `test`, or `prod`).
- Confirm CI is green for that SHA.
- Obtain the approval required for the target GitHub environment. Production must
  retain required reviewers in GitHub environment protection.
- Confirm the Azure and Etsy secret references required by `deploy.yml` are present.
  Never copy secret values into workflow inputs, logs, issues, or pull requests.

## Deploy

1. Open the **Deploy to Azure** workflow in GitHub Actions.
2. Select **Run workflow**, choose the exact branch or commit, and select the target
   environment explicitly.
3. Wait for environment approval and every workflow step to complete.
4. Record the workflow URL, deployed commit SHA, environment, frontend URL, and API URL.
5. Verify the API health endpoint and frontend smoke check reported by the workflow.
6. For production, exercise sign-in and one non-destructive workspace read before
   declaring the deployment complete.

## Roll Back Application Code

1. Stop further deployments and record the failed workflow URL and symptoms.
2. Find the last known-good deployment for the same environment and verify its commit
   SHA from GitHub Actions evidence.
3. Run **Deploy to Azure** manually at that known-good ref for the same environment.
4. Re-run the API, frontend, sign-in, and workspace-read verification above.
5. Record the rollback workflow URL and restored commit SHA in the incident or issue.

## Infrastructure And Data Safety

Do not delete resources, force state changes, rotate secrets, or reverse data migrations
as an automatic rollback. Infrastructure or data rollback requires an explicit owner
decision, an impact assessment, and a tested recovery command. When application rollback
cannot restore service safely, leave the deployment blocked and escalate with the failing
workflow, current resource state, logs, and proposed recovery steps.
