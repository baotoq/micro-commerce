import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AuthGuard } from './auth/auth.guard';

@NgModule({
  declarations: [],
  imports: [CommonModule],
})
export class SharedModule {
  static forRoot(): ModuleWithProviders<SharedModule> {
    return {
      providers: [AuthGuard],
      ngModule: SharedModule,
    };
  }
}
