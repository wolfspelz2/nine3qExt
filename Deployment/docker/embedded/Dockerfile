FROM node AS build

WORKDIR /app
COPY ./ChromeExt ./

RUN rm -rf node_modules
RUN rm -rf dist
RUN npm install
RUN npm run build:embedded


FROM nginx:1.19.0-alpine
WORKDIR /app
COPY --from=build /app/dist /var/www/v1
