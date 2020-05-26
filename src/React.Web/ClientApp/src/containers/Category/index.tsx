import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { Row, Col } from "react-bootstrap";
import Product from "./Product";

import categoryService, { CategoryResponse } from "../../services/category-service";

const Category = () => {
  const { id } = useParams<{ id: string }>();
  const [category, setCategory] = useState<CategoryResponse>();

  useEffect(() => {
    const fetchCategory = async () => {
      var response = await categoryService.findAsync(+id);
      setCategory(response);
    };

    fetchCategory();
  }, [id, setCategory]);

  return (
    <div>
      {category && (
        <div>
          <Row>
            {category.products.map((product) => (
              <Col md={3} key={product.id} className="mb-3">
                <Product product={product} />
              </Col>
            ))}
          </Row>
        </div>
      )}
    </div>
  );
};

export default Category;
