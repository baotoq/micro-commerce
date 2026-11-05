import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { useNavigate, useParams } from 'react-router'
import { ApiError, getJson, sendJson } from '../api'
import { errorMessage, Field, fieldErrors } from '../lib/forms'
import { buildVariantGrid, type OptionInput, type VariantRow } from '../lib/variantGrid'
import { flatten, useCategoryTree } from '../marketplace/categories'
import { useSession } from '../session'
import { ImageManager } from './ImageManager'
import { conflictMessages, type MerchantProduct } from './products'

type FormState = { name: string; description: string; categoryId: string; options: OptionInput[]; rows: VariantRow[] }

const emptyForm: FormState = {
  name: '',
  description: '',
  categoryId: '',
  options: [],
  rows: buildVariantGrid([], []),
}

function fromProduct(p: MerchantProduct): FormState {
  return {
    name: p.name,
    description: p.description ?? '',
    categoryId: p.categoryId,
    options: p.options.map((o) => ({ name: o.name, values: [...o.values] })),
    rows: p.variants.map((v) => ({
      optionValues: v.optionValues,
      price: String(v.price),
      stock: String(v.stock),
      weightGrams: String(v.weightGrams),
    })),
  }
}

/** Create or edit a Product: details, up to two Options, the Variant grid and images (User Stories 2, 4, 5). */
export function ProductForm() {
  const { id } = useParams()
  const { token } = useSession()
  const navigate = useNavigate()
  const { tree } = useCategoryTree()
  const leaves = useMemo(() => flatten(tree).filter((c) => c.isLeaf), [tree])
  const [product, setProduct] = useState<MerchantProduct | null>(null)
  const [form, setForm] = useState<FormState>(emptyForm)
  const [error, setError] = useState<unknown>(null)
  const [busy, setBusy] = useState(false)
  const [saved, setSaved] = useState(false)

  useEffect(() => {
    if (!id || !token) return
    getJson<MerchantProduct>(`/api/merchant/products/${id}`, token).then((p) => {
      setProduct(p)
      setForm(fromProduct(p))
    }, setError)
  }, [id, token])

  const readOnly = product?.status === 'Removed'

  const setOptions = (options: OptionInput[]) => setForm((f) => ({ ...f, options, rows: buildVariantGrid(options, f.rows) }))
  const setRow = (index: number, field: 'price' | 'stock' | 'weightGrams', value: string) =>
    setForm((f) => ({ ...f, rows: f.rows.map((r, i) => (i === index ? { ...r, [field]: value } : r)) }))

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setBusy(true)
    setError(null)
    setSaved(false)
    const body = {
      name: form.name,
      description: form.description || null,
      categoryId: form.categoryId || null,
      options: form.options.filter((o) => o.name.trim() && o.values.length > 0),
      variants: form.rows.map((r) => ({
        optionValues: r.optionValues,
        price: r.price === '' ? null : Number(r.price),
        stock: r.stock === '' ? null : Number(r.stock),
        weightGrams: r.weightGrams === '' ? null : Number(r.weightGrams),
      })),
    }
    try {
      if (product) {
        const updated = await sendJson<MerchantProduct>(`/api/merchant/products/${product.id}`, {
          token,
          method: 'PUT',
          body: { ...body, version: product.version },
        })
        setProduct(updated)
        setForm(fromProduct(updated))
        setSaved(true)
      } else {
        const created = await sendJson<MerchantProduct>('/api/merchant/products', { token, body })
        navigate(`/merchant/products/${created.id}`)
      }
    } catch (e) {
      setError(e)
    } finally {
      setBusy(false)
    }
  }

  const errors = fieldErrors(error)
  const message =
    error instanceof ApiError && error.type
      ? (conflictMessages[error.type] ?? errorMessage(error))
      : error !== null && Object.keys(errors).length === 0
        ? errorMessage(error)
        : null

  return (
    <section>
      <h3>{product ? `Edit ${product.name}` : 'New Product'}</h3>
      {product && <p>Status: {product.status === 'Removed' ? 'Removed by a Platform Operator' : product.status}</p>}
      {message && <p role="alert">{message}</p>}
      {saved && <p>Saved.</p>}
      <form onSubmit={(e) => void submit(e)}>
        <fieldset disabled={readOnly || busy}>
          <Field label="Name" field="name" errors={errors}>
            <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required maxLength={200} />
          </Field>
          <Field label="Description" field="description" errors={errors}>
            <textarea value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} maxLength={5000} />
          </Field>
          <Field label="Category" field="categoryId" errors={errors}>
            <select value={form.categoryId} onChange={(e) => setForm({ ...form, categoryId: e.target.value })} required>
              <option value="">Choose a Category…</option>
              {leaves.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.path}
                </option>
              ))}
            </select>
          </Field>
          <OptionEditor options={form.options} onChange={setOptions} errors={errors} />
          <VariantGrid rows={form.rows} onChange={setRow} errors={errors} />
          <button type="submit">{product ? 'Save' : 'Create Product'}</button>
        </fieldset>
      </form>
      {product && !readOnly && <ImageManager product={product} onChange={setProduct} />}
    </section>
  )
}

function OptionEditor({ options, onChange, errors }: { options: OptionInput[]; onChange: (o: OptionInput[]) => void; errors: Record<string, string[]> }) {
  const [drafts, setDrafts] = useState<string[]>([])
  const update = (i: number, option: OptionInput) => onChange(options.map((o, j) => (j === i ? option : o)))

  return (
    <fieldset>
      <legend>Options (at most 2)</legend>
      {(errors['options'] ?? []).map((m) => (
        <small key={m} role="alert" style={{ display: 'block', color: 'crimson' }}>
          {m}
        </small>
      ))}
      {options.map((option, i) => (
        <div key={i}>
          <input aria-label={`Option ${i + 1} name`} placeholder="e.g. Colour" value={option.name} onChange={(e) => update(i, { ...option, name: e.target.value })} />
          {option.values.map((value) => (
            <button type="button" key={value} title="Remove value" onClick={() => update(i, { ...option, values: option.values.filter((v) => v !== value) })}>
              {value} ×
            </button>
          ))}
          <input
            aria-label={`Option ${i + 1} new value`}
            placeholder="Add a value, then Enter"
            value={drafts[i] ?? ''}
            onChange={(e) => setDrafts((d) => Object.assign([...d], { [i]: e.target.value }))}
            onKeyDown={(e) => {
              const value = (drafts[i] ?? '').trim()
              if (e.key !== 'Enter') return
              e.preventDefault()
              if (value && !option.values.some((v) => v.toLowerCase() === value.toLowerCase())) update(i, { ...option, values: [...option.values, value] })
              setDrafts((d) => Object.assign([...d], { [i]: '' }))
            }}
          />
          <button type="button" onClick={() => onChange(options.filter((_, j) => j !== i))}>
            Remove option
          </button>
        </div>
      ))}
      {options.length < 2 && (
        <button type="button" onClick={() => onChange([...options, { name: '', values: [] }])}>
          Add option
        </button>
      )}
    </fieldset>
  )
}

function VariantGrid({
  rows,
  onChange,
  errors,
}: {
  rows: VariantRow[]
  onChange: (index: number, field: 'price' | 'stock' | 'weightGrams', value: string) => void
  errors: Record<string, string[]>
}) {
  return (
    <fieldset>
      <legend>Variants</legend>
      {(errors['variants'] ?? []).map((m) => (
        <small key={m} role="alert" style={{ display: 'block', color: 'crimson' }}>
          {m}
        </small>
      ))}
      <table>
        <thead>
          <tr>
            <th>Variant</th>
            <th>Price</th>
            <th>Stock</th>
            <th>Weight (g)</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((row, i) => (
            <tr key={row.optionValues.join('/') || 'default'}>
              <td>{row.optionValues.length ? row.optionValues.join(' / ') : 'Default'}</td>
              {(['price', 'stock', 'weightGrams'] as const).map((field) => (
                <td key={field}>
                  <input type="number" aria-label={`${row.optionValues.join(' / ') || 'Default'} ${field}`} value={row[field]} onChange={(e) => onChange(i, field, e.target.value)} />
                  {(errors[`variants[${i}].${field}`] ?? []).map((m) => (
                    <small key={m} role="alert" style={{ display: 'block', color: 'crimson' }}>
                      {m}
                    </small>
                  ))}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </fieldset>
  )
}
