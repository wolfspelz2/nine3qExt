#!/bin/bash



git clone git@github.com:wolfspelz/nine3q.git /tmp/build



cd /tmp/build
git checkout feature/deployment
git pull

docker build -f Dockerfile.webex -t docker.k8s.sui.li/n3q-webex:$1 .
docker push docker.k8s.sui.li/n3q-webex:$1
kubectl set image deployment/n3q-webex n3q-webex=docker.k8s.sui.li/n3q-webex:$1 --record --namespace=n3q-prod


docker build -f Dockerfile.web -t docker.k8s.sui.li/n3q-web:$1 .
docker push docker.k8s.sui.li/n3q-web:$1
kubectl set image deployment/n3q-web n3q-web=docker.k8s.sui.li/n3q-web:$1 --record --namespace=n3q-prod

docker build -f Dockerfile.silo -t docker.k8s.sui.li/n3q-silo:$1 .
docker push docker.k8s.sui.li/n3q-silo:$1
kubectl set image deployment/n3q-silo n3q-silo=docker.k8s.sui.li/n3q-silo:$1 --record --namespace=n3q-prod

docker build -f Dockerfile.xmpp -t docker.k8s.sui.li/n3q-xmpp:$1 .
docker push docker.k8s.sui.li/n3q-xmpp:$1
kubectl set image deployment/n3q-xmpp n3q-xmpp=docker.k8s.sui.li/n3q-xmpp:$1 --record --namespace=n3q-prod

