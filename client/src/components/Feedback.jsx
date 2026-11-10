export default function Feedback({ loading, error, onRetry }) {
  return (
    <>
      {loading && <p role="status">Consultando tus datos…</p>}
      {error && (
        <div role="alert" className="error">
          {error}{" "}
          <button type="button" onClick={onRetry}>
            Reintentar
          </button>
        </div>
      )}
    </>
  );
}
