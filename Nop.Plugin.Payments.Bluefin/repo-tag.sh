REPO_VERSION=`node -e "console.log(require('./plugin.json').Version)"` && echo TAG: v$REPO_VERSION && git commit -a -m v$REPO_VERSION && git push && git tag v$REPO_VERSION && git push --tags;
