export default function Feedback({ loading, error, onRetry }) {
  return (
    <>
      {loading && (
        <p className="notice" role="status">
          Consultando tus finanzas…
        </p>
      )}
      {error && (
        <div role="alert" className="notice error">
          {error}{" "}
          <button type="button" onClick={onRetry}>
            Reintentar
          </button>
        </div>
      )}
    </>
  );
}
