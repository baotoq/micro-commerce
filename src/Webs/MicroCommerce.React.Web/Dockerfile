FROM mhart/alpine-node

EXPOSE 3000

WORKDIR /app

COPY yarn.lock ./
COPY package*.json ./
RUN yarn install --frozen-lockfile

COPY . .

CMD ["yarn", "start:dev"]
