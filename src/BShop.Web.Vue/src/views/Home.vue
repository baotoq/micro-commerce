<template>
  <div class="home">
    <img alt="Vue logo" src="../assets/logo.png" />
    <hello-world msg="Welcome to Your Vue.js App"></hello-world>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import HelloWorld from "@/components/HelloWorld.vue";

@Component({ name: "Home", components: { HelloWorld } })
export default class Home extends Vue {}
</script>
