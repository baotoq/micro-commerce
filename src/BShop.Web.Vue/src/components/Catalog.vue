<template>
  <div>
    <div v-for="catalog in catalogs" :key="catalog.id">
      {{ catalog.id }} {{ catalog.name }}
    </div>
  </div>
</template>

<script lang="ts">
import { Component, Vue, Prop } from "vue-property-decorator";

@Component
export default class Catalog extends Vue {
  @Prop({ default: () => [] })
  catalogs: any;
}
</script>
