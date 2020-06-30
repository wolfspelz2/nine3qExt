#!/bin/bash

n=$(< incr)
let "n=n+1"
echo $n > incr

git clone git@github.com:wolfspelz/nine3q.git /tmp/build



cd /tmp/build
git checkout feature/deployment
git pull

#docker build -f Deployment/docker/webex/Dockerfile -t docker.k8s.sui.li/n3q-webex:$n .
#docker push docker.k8s.sui.li/n3q-webex:$n
#kubectl set image deployment/n3q-webex n3q-webex=docker.k8s.sui.li/n3q-webex:$n --record --namespace=n3q-prod


#docker build -f Deployment/docker/web/Dockerfile -t docker.k8s.sui.li/n3q-web:$n .
#docker push docker.k8s.sui.li/n3q-web:$n
#kubectl set image deployment/n3q-web n3q-web=docker.k8s.sui.li/n3q-web:$n --record --namespace=n3q-prod

#docker build -f Deployment/docker/silo/Dockerfile -t docker.k8s.sui.li/n3q-silo:$n .
#docker push docker.k8s.sui.li/n3q-silo:$n
#kubectl set image deployment/n3q-silo n3q-silo=docker.k8s.sui.li/n3q-silo:$n --record --namespace=n3q-prod

#docker build -f Deployment/docker/xmpp/Dockerfile -t docker.k8s.sui.li/n3q-xmpp:$n .
#docker push docker.k8s.sui.li/n3q-xmpp:$n
#kubectl set image deployment/n3q-xmpp n3q-xmpp=docker.k8s.sui.li/n3q-xmpp:$n --record --namespace=n3q-prod

docker build -f Deployment/docker/prosody-xmpp/Dockerfile -t docker.k8s.sui.li/prosody-xmpp:$n .
docker push docker.k8s.sui.li/prosody-xmpp:$n
kubectl set image deployment/prosody-xmpp prosody-xmpp=docker.k8s.sui.li/prosody-xmpp:$n --record --namespace=default
