#!/bin/bash
git clone git@github.com:wolfspelz/nine3q.git /tmp/build

cd /tmp/build
git checkout feature/deployment
git pull

#docker build -f Dockerfile.webex -t docker.k8s.sui.li/n3q-webex:2 .
#docker push docker.k8s.sui.li/n3q-webex:2

#docker build -f Dockerfile.web -t docker.k8s.sui.li/n3q-web:2 .
#docker push docker.k8s.sui.li/n3q-web:2

#docker build -f Dockerfile.silo -t docker.k8s.sui.li/n3q-silo:2 .
#docker push docker.k8s.sui.li/n3q-silo:2

docker build -f Dockerfile.xmpp -t docker.k8s.sui.li/n3q-xmpp:2 .
docker push docker.k8s.sui.li/n3q-xmpp:2

#kubectl set image deployment/n3q-webex n3q-webex=docker.k8s.sui.li/n3q-webex:3 --record
