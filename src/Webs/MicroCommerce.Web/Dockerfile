FROM mhart/alpine-node

EXPOSE 3000

WORKDIR /app

COPY . .

RUN npm install --frozen-lockfile

RUN npm run build


CMD ["yarn", "start"]