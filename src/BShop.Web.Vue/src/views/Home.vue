<template>
  <div class="home">
    <catalog :catalogs="catalogs"></catalog>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { namespace } from "vuex-class";
import Catalog from "@/components/Catalog.vue";

const catalogModule = namespace("catalog");

@Component({ name: "Home", components: { Catalog } })
export default class Home extends Vue {
  @catalogModule.State("catalogs")
  private catalogs!: any;

  @catalogModule.Action("fetchAll")
  private featchAllCatalogs!: () => Promise<void>;

  async mounted() {
    await this.featchAllCatalogs();
  }
}
</script>
