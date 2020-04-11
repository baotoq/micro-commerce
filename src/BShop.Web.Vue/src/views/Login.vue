<template>
  <div>
    Login
    <div>
      <label for="userName">Username</label>
      <input type="text" name="userName" v-model="userName" />
    </div>
    <div>
      <label for="password">Password</label>
      <input type="text" name="password" v-model="password" />
    </div>
    <button @click="login({ userName, password })">Login</button>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { namespace } from "vuex-class";
import { LoginPayload } from "../store/types";

const authModule = namespace("auth");

@Component({ name: "Login" })
export default class Login extends Vue {
  private userName = "";
  private password = "";

  @authModule.Action
  private login!: ({ userName, password }: LoginPayload) => void;
}
</script>

<style></style>
