# Release Process

This project follows [Gitflow](https://nvie.com/posts/a-successful-git-branching-model/) and uses [GitVersion](https://gitversion.net/) (ContinuousDelivery mode) for automatic semantic versioning. NuGet publishing is handled by GitHub Actions on push to `master`.

## Branch Overview

| Branch | Purpose |
|---|---|
| `master` | Production releases. Pushes here trigger NuGet publish. |
| `develop` | Integration branch for the next release. |
| `feature/*` | Feature branches off `develop`. |
| `release/*` | Release stabilization branches off `develop`. |

## Release Steps

### 1. Ensure `develop` is ready

- All feature branches for the release are merged into `develop`.
- CI is green on `develop`.

### 2. Create a release branch

```bash
git checkout develop
git pull origin develop
git checkout -b release/X.Y.Z
```

### 3. Finalize the release

On the release branch, make release-only changes:

- **Update target frameworks** in `src/Gotenberg.Sharp.Api.Client/Gotenberg.Sharp.Api.Client.csproj` if adding/dropping TFMs.
- **Update `PackageReleaseNotes`** in the csproj with a summary of the release.
- **Update `Description`** in the csproj if the library's capabilities have changed.
- **Update `CHANGES.MD`** with a full release section listing breaking changes, features, bug fixes, and documentation changes.
- **Update `README.md`** with a release callout and any new/changed usage examples.
- Build and run tests locally:

```bash
dotnet build
dotnet test
```

### 4. Merge release branch into `master`

```bash
git checkout master
git pull origin master
git merge --no-ff release/X.Y.Z
git tag X.Y.Z
git push origin master --tags
```

This push triggers the GitHub Actions workflow which will:
1. Build and test against a Gotenberg docker container with basic auth.
2. Calculate the package version via GitVersion.
3. Pack the NuGet package (with symbols).
4. Publish to nuget.org.

### 5. Merge release branch back into `develop`

```bash
git checkout develop
git pull origin develop
git merge --no-ff release/X.Y.Z
git push origin develop
```

### 6. Clean up

```bash
git branch -d release/X.Y.Z
git push origin --delete release/X.Y.Z
```

### 7. Create a GitHub Release

- Go to **Releases** > **Draft a new release**.
- Choose the `X.Y.Z` tag.
- Title: `vX.Y.Z`
- Body: Copy the corresponding section from `CHANGES.MD`.
- Publish.

## Hotfix Process

For critical fixes to a released version:

```bash
git checkout master
git checkout -b hotfix/X.Y.Z+1
# make fix, commit
git checkout master
git merge --no-ff hotfix/X.Y.Z+1
git tag X.Y.Z+1
git push origin master --tags

git checkout develop
git merge --no-ff hotfix/X.Y.Z+1
git push origin develop

git branch -d hotfix/X.Y.Z+1
```

## CI/CD Details

- **Workflow**: `.github/workflows/deploy.yml`
- **Triggers**: All pushes and pull requests (build + test). NuGet publish only on push to `master`.
- **Versioning**: GitVersion with `ContinuousDelivery` mode (`GitVersion.yml`).
- **NuGet API key**: Stored as `NUGETKEY` repository secret.
- **Test infrastructure**: Gotenberg v8 docker container with basic auth (`testuser`/`testpass`).
