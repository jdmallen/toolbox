#!/usr/bin/env bash
# Packs and publishes the JDMallen.Toolbox NuGet packages to the private
# GitHub Packages feed. It only packs and pushes — it does not bump the
# version, commit, or tag.
#
# Versioning/release flow: bump the single global <Version> in
# Directory.Build.props in your PR. When the PR merges to main,
# .github/workflows/release.yml runs this script and then creates the matching
# git tag + GitHub release. Routine merges that don't bump <Version> are no-ops
# (the workflow skips when the tag already exists).
#
# Packable projects are discovered dynamically: any src/**/*.csproj that
# declares a <PackageId> is packed, so the list stays correct as projects come
# and go. (Test projects and the TestWorker have no <PackageId> and are skipped.)
#
#   ./scripts/publish-nuget.sh              # pack + push the current version
#   ./scripts/publish-nuget.sh --no-push    # pack only; artifacts in nupkgs/
#   ./scripts/publish-nuget.sh -v 3.1.0     # pack/push an explicit version
#
# Authentication:
#   Pushing needs a GitHub PAT (classic) with the write:packages scope, or in
#   CI the automatic GITHUB_TOKEN with `packages: write` permission. Provide it
#   via --token, via $GITHUB_PACKAGES_TOKEN / $GITHUB_TOKEN, or by setting
#   GITHUB_PACKAGES_TOKEN in a local .env file (auto-loaded; managed with
#   direnv here). The feed inherits visibility from the repo named in
#   <RepositoryUrl>, so packages published from this private repo are private.

set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"

# Load local secrets (e.g. GITHUB_PACKAGES_TOKEN) if present, exporting them so
# dotnet also sees them for nuget.config %GITHUB_PACKAGES_TOKEN% substitution.
if [[ -f "$repo_root/.env" ]]; then
	set -a
	# shellcheck source=/dev/null
	source "$repo_root/.env"
	set +a
fi

props="Directory.Build.props"
output_dir="nupkgs"
source_url="https://nuget.pkg.github.com/jdmallen/index.json"
token="${GITHUB_PACKAGES_TOKEN:-${GITHUB_TOKEN:-}}"

version=""     # explicit version from -v; otherwise read from $props
do_push="yes"

print_help() {
	sed -n '2,/^set -euo/p' "$0" | sed '$d; s/^# \{0,1\}//'
}

# --- Parse arguments ---------------------------------------------------------
while [[ $# -gt 0 ]]; do
	case "$1" in
		-h|--help)    print_help; exit 0 ;;
		-v|--version) version="${2:?--version requires a value}"; shift 2 ;;
		--no-push)    do_push="no"; shift ;;
		--source)     source_url="${2:?--source requires a value}"; shift 2 ;;
		--token)      token="${2:?--token requires a value}"; shift 2 ;;
		--output)     output_dir="${2:?--output requires a value}"; shift 2 ;;
		*) echo "Error: unknown argument '$1'." >&2; exit 1 ;;
	esac
done

# --- Resolve the version -----------------------------------------------------
if [[ -z "$version" ]]; then
	version="$(grep -oP '(?<=<Version>)[^<]+' "$props" | head -n1 || true)"
fi
if [[ -z "$version" ]]; then
	echo "Error: could not determine version (no <Version> in $props)." >&2
	exit 1
fi

if [[ "$do_push" == "yes" && -z "$token" ]]; then
	echo "Error: no push token. Set \$GITHUB_PACKAGES_TOKEN / \$GITHUB_TOKEN or" >&2
	echo "       pass --token, or rerun with --no-push to pack only." >&2
	exit 1
fi

# --- Pack --------------------------------------------------------------------
mapfile -t packable < <(grep -rl '<PackageId>' src --include='*.csproj' | sort)
if [[ ${#packable[@]} -eq 0 ]]; then
	echo "Error: found no packable projects (none declare <PackageId>)." >&2
	exit 1
fi

rm -rf "$output_dir"
mkdir -p "$output_dir"

echo ">>> Packing ${#packable[@]} projects at v$version"
for project in "${packable[@]}"; do
	echo "    - $project"
	dotnet pack "$project" \
		--configuration Release \
		-p:Version="$version" \
		--output "$output_dir" \
		--nologo \
		--verbosity minimal
done
echo ""
echo "Packed:"
ls -1 "$output_dir"/*.nupkg

# --- Push --------------------------------------------------------------------
if [[ "$do_push" == "yes" ]]; then
	echo ""
	echo ">>> Pushing to $source_url"
	for package in "$output_dir"/*.nupkg; do
		echo "    - $(basename "$package")"
		dotnet nuget push "$package" \
			--api-key "$token" \
			--source "$source_url" \
			--skip-duplicate
	done
else
	echo ""
	echo ">>> Skipping push (--no-push)."
fi

echo ""
echo "Done."
