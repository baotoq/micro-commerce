import { useState, type FormEvent } from 'react'
import { Field } from '../lib/forms'

export type ShopFormValues = {
  shopName: string
  description: string
  pickupAddress: { street: string; district: string; province: string }
}

export const emptyShop: ShopFormValues = { shopName: '', description: '', pickupAddress: { street: '', district: '', province: '' } }

/** The Shop name, description and Pickup Address, shared by opening and editing a Shop. */
export function ShopForm({
  initial,
  submitLabel,
  errors,
  busy,
  onSubmit,
}: {
  initial: ShopFormValues
  submitLabel: string
  errors: Record<string, string[]>
  busy: boolean
  onSubmit: (values: ShopFormValues) => void
}) {
  const [values, setValues] = useState(initial)
  const setAddress = (part: keyof ShopFormValues['pickupAddress'], value: string) =>
    setValues((v) => ({ ...v, pickupAddress: { ...v.pickupAddress, [part]: value } }))

  const submit = (e: FormEvent) => {
    e.preventDefault()
    onSubmit(values)
  }

  return (
    <form onSubmit={submit}>
      <Field label="Shop name" field="shopName" errors={errors}>
        <input value={values.shopName} onChange={(e) => setValues({ ...values, shopName: e.target.value })} required minLength={3} maxLength={50} />
      </Field>
      <Field label="Description" field="description" errors={errors}>
        <textarea value={values.description} onChange={(e) => setValues({ ...values, description: e.target.value })} maxLength={2000} />
      </Field>
      <fieldset>
        <legend>Pickup Address</legend>
        <Field label="Street" field="pickupAddress.street" errors={errors}>
          <input value={values.pickupAddress.street} onChange={(e) => setAddress('street', e.target.value)} required maxLength={200} />
        </Field>
        <Field label="District" field="pickupAddress.district" errors={errors}>
          <input value={values.pickupAddress.district} onChange={(e) => setAddress('district', e.target.value)} required maxLength={100} />
        </Field>
        <Field label="Province" field="pickupAddress.province" errors={errors}>
          <input value={values.pickupAddress.province} onChange={(e) => setAddress('province', e.target.value)} required maxLength={100} />
        </Field>
      </fieldset>
      <button type="submit" disabled={busy}>
        {submitLabel}
      </button>
    </form>
  )
}
